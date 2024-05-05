namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;
    using Model;

    [Component(EnLifestyle.Singleton)]
    internal class SelectLinqExpressionVisitor : ILinqExpressionVisitor,
                                                 ICollectionResolvable<ILinqExpressionVisitor>
    {
        private readonly IModelProvider _modelProvider;

        public SelectLinqExpressionVisitor(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        public bool TryVisit(
            ExpressionVisitor visitor,
            TranslationContext context,
            Expression expression)
        {
            if (expression is not System.Linq.Expressions.MethodCallExpression methodCallExpression)
            {
                return false;
            }

            var method = methodCallExpression.Method.GenericMethodDefinitionOrSelf();

            if (method == LinqMethods.QueryableSelect())
            {
                var itemType = methodCallExpression.Type.ExtractQueryableItemType();

                visitor.Visit(methodCallExpression.Arguments[0]);
                var source = context.SqlExpression;
                var sourceItemType = GetSourceItemType(source);
                var parameterExpression = new Expressions.ParameterExpression(sourceItemType, context.NextLambdaParameterName());
                var unnamedSource = source is QuerySourceExpression ? source : new ParenthesesExpression(context.SqlExpression);
                var namedSourceExpression = new NamedSourceExpression(sourceItemType, unnamedSource, parameterExpression);

                IReadOnlyCollection<ISqlExpression> expressions;

                using (context.OpenParameterScope(namedSourceExpression.Parameter))
                {
                    visitor.Visit(methodCallExpression.Arguments[1]);
                    expressions = context.SqlExpression is Expressions.NewExpression newExpression
                        ? newExpression.Parameters
                        : new[] { context.SqlExpression };
                }

                if (expressions.Count == 1
                    && expressions.Single() is ColumnExpression columnExpression
                    && columnExpression.Type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
                {
                    /*
                     * handle relation selector
                     */

                    // TODO: try to remove
                    /*var relationParameterExpression = new Expressions.ParameterExpression(columnExpression.Type, namedSourceExpression.Parameter.Name);
                    expressions = RepositoryAllLinqExpressionVisitor
                        .SelectAll(_modelProvider, columnExpression.Type, relationParameterExpression)
                        .Select(it =>
                        {
                            var relationColumnExpression = new ColumnExpression(it.Type, it.Member, it.Source, $"{columnExpression.Name}_{it.Name}");
                            return new RenameExpression(it.Type, it.Name, relationColumnExpression);
                        })
                        .ToList();*/
                }

                var projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions, null, null);

                if (BuildJoinExpression(context, _modelProvider, projectionExpression) is { } joinProjectionExpression)
                {
                    projectionExpression = joinProjectionExpression;
                }

                context.Remember(projectionExpression);

                return true;
            }

            return false;
        }

        // TODO: private
        internal static ISqlExpression? GetSource(ISqlExpression source)
        {
            return source switch
            {
                DeleteExpression => default,
                JoinExpression => default,
                NamedSourceExpression namedSourceExpression => namedSourceExpression.Source,
                ProjectionExpression projectionExpression => GetSource(projectionExpression.Source),
                QuerySourceExpression => default,
                UpdateExpression => default,
                _ => throw new NotSupportedException(source.GetType().FullName)
            };
        }

        // TODO: private
        internal static Type GetSourceItemType(ISqlExpression source)
        {
            return source switch
            {
                DeleteExpression deleteExpression => deleteExpression.ItemType,
                ParenthesesExpression parenthesesExpression => GetSourceItemType(parenthesesExpression.Source),
                ProjectionExpression projectionExpression => projectionExpression.ItemType,
                QuerySourceExpression querySourceExpression => querySourceExpression.ItemType,
                UpdateExpression updateExpression => updateExpression.ItemType,
                _ => throw new NotSupportedException(source.GetType().FullName)
            };
        }

        // TODO: private
        internal static ProjectionExpression? BuildJoinExpression(
            TranslationContext context,
            IModelProvider modelProvider,
            ISqlExpression sourceExpression)
        {
            var sourceItemType = GetSourceItemType(sourceExpression);

            if (!sourceItemType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
            {
                return null;
            }

            var relations = modelProvider
                .Tables[sourceItemType]
                .Columns
                .Select(it => it.Value)
                .Where(column => column.IsRelation)
                .Select(column => column.Relation)
                .ToList();

            if (!relations.Any())
            {
                return null;
            }

            var sourceParameterExpression = new Expressions.ParameterExpression(sourceItemType, context.NextLambdaParameterName());
            NamedSourceExpression sourceNamedSourceExpression;
            IReadOnlyCollection<ISqlExpression> sourceExpressions;
            FilterExpression? filterExpression;
            OrderByExpression? orderByExpression;

            switch (sourceExpression)
            {
                case ProjectionExpression sourceProjectionExpression:
                {
                    filterExpression = sourceProjectionExpression.FilterExpression != null
                        ? new FilterExpression(sourceProjectionExpression.FilterExpression.ItemType, WhereLinqExpressionVisitor.ReplaceParameterSqlExpressionVisitor.Replace(sourceProjectionExpression.FilterExpression.Predicate, sourceParameterExpression))
                        : null;
                    sourceProjectionExpression.FilterExpression = null;
                    orderByExpression = sourceProjectionExpression.OrderByExpression != null
                        ? new OrderByExpression(sourceProjectionExpression.OrderByExpression.Expressions.Select(orderByExpressionExpression => (OrderByExpressionExpression)WhereLinqExpressionVisitor.ReplaceParameterSqlExpressionVisitor.Replace(orderByExpressionExpression, sourceParameterExpression)).ToList())
                        : null;
                    sourceProjectionExpression.OrderByExpression = null;
                    var sourceParenthesesExpression = new ParenthesesExpression(sourceProjectionExpression);
                    sourceNamedSourceExpression = new NamedSourceExpression(sourceItemType, sourceParenthesesExpression, sourceParameterExpression);
                    sourceExpressions = RepositoryAllLinqExpressionVisitor.SelectAll(modelProvider, sourceItemType, sourceParameterExpression);

                    break;
                }

                case QuerySourceExpression querySourceExpression:
                {
                    filterExpression = null;
                    orderByExpression = null;
                    sourceNamedSourceExpression = new NamedSourceExpression(sourceItemType, querySourceExpression, sourceParameterExpression);
                    sourceExpressions = RepositoryAllLinqExpressionVisitor.SelectAll(modelProvider, sourceItemType, sourceParameterExpression);
                    break;
                }

                default:
                {
                    throw new NotSupportedException(sourceExpression.GetType().FullName);
                }
            }

            ISqlExpression joinAccumulator = sourceNamedSourceExpression;
            IEnumerable<ISqlExpression> expressions = sourceExpressions.ToList();

            foreach (var relation in relations)
            {
                var targetItemType = relation.Target;
                var targetQuerySourceExpression = new QuerySourceExpression(targetItemType);
                var targetParameterExpression = new Expressions.ParameterExpression(targetItemType, context.NextLambdaParameterName());
                var targetNamedSourceExpression = new NamedSourceExpression(targetItemType, targetQuerySourceExpression, targetParameterExpression);
                var targetExpressions = RepositoryAllLinqExpressionVisitor
                    .SelectAll(modelProvider, targetItemType, targetParameterExpression)
                    .Select(sqlExpression => sqlExpression is ColumnExpression columnExpression
                        ? new RenameExpression(columnExpression.Type, $"{relation.Property.Reflected.Name}_{columnExpression.Name}", columnExpression)
                        : sqlExpression)
                    .ToList();
                var onExpression = BuildJoinOnExpression(modelProvider, relation, sourceParameterExpression, targetParameterExpression);
                joinAccumulator = new JoinExpression(joinAccumulator, targetNamedSourceExpression, onExpression);

                expressions = expressions.Concat(targetExpressions);
            }

            var projectionExpression = new ProjectionExpression(null!, joinAccumulator, expressions.ToList(), filterExpression, orderByExpression);

            return projectionExpression;

            static Expressions.BinaryExpression BuildJoinOnExpression(
                IModelProvider modelProvider,
                Relation relation,
                Expressions.ParameterExpression sourceParameterExpression,
                Expressions.ParameterExpression targetParameterExpression)
            {
                var targetPrimaryKeyColumn = modelProvider
                    .Tables[relation.Target]
                    .Columns[nameof(IUniqueIdentified.PrimaryKey)];

                return new Expressions.BinaryExpression(
                    typeof(bool),
                    BinaryOperator.Equal,
                    new ColumnExpression(
                        targetPrimaryKeyColumn.Type,
                        relation.Target.Column(nameof(IUniqueIdentified.PrimaryKey)).Reflected,
                        targetParameterExpression),
                    new ColumnExpression(
                        targetPrimaryKeyColumn.Type,
                        relation.Property.Reflected,
                        sourceParameterExpression));
            }
        }
    }
}