namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
                var unnamedSource = source is QuerySourceExpression ? source : new ParenthesesExpression(source);
                var namedSourceExpression = new NamedSourceExpression(sourceItemType, unnamedSource, parameterExpression);

                IReadOnlyCollection<ISqlExpression> expressions;

                using (context.OpenParametersScope(parameterExpression))
                {
                    visitor.Visit(methodCallExpression.Arguments[1]);
                    expressions = context.SqlExpression is Expressions.NewExpression newExpression
                        ? newExpression.Parameters
                        : new[] { context.SqlExpression };
                }

                var relations = RelationsExpressionVisitor.ExtractRelations(_modelProvider, methodCallExpression.Arguments[1]);

                ProjectionExpression projectionExpression;

                if (TryBuildJoinExpression(context, _modelProvider, namedSourceExpression, relations, out var joinExpression, out var relationExpressions, out _))
                {
                    expressions = expressions.Concat(relationExpressions).ToList();
                    projectionExpression = new ProjectionExpression(itemType, joinExpression, expressions, null, null);
                }
                else
                {
                    projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions, null, null);
                }

                context.Remember(projectionExpression);

                return true;
            }

            return false;
        }

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

        internal static IReadOnlyCollection<ColumnExpression> SelectAll(
            IModelProvider modelProvider,
            Type type,
            Expressions.ParameterExpression parameterExpression)
        {
            if (!type.IsClass
                || type.IsPrimitive()
                || type.IsCollection())
            {
                throw new InvalidOperationException(nameof(SelectAll));
            }

            return modelProvider
                .Columns(type)
                .Values
                .Where(column => !column.IsMultipleRelation)
                .Select(column => column.BuildExpression(parameterExpression))
                .ToList();
        }

        internal static bool TryBuildJoinExpression(
            TranslationContext context,
            IModelProvider modelProvider,
            NamedSourceExpression sourceExpression,
            IReadOnlyCollection<Relation> relations,
            [NotNullWhen(true)] out JoinExpression? joinExpression,
            [NotNullWhen(true)] out IReadOnlyCollection<ISqlExpression>? relationExpressions,
            [NotNullWhen(true)] out IReadOnlyCollection<Expressions.ParameterExpression>? joinParameterExpressions)
        {
            if (!relations.Any())
            {
                joinExpression = null;
                relationExpressions = null;
                joinParameterExpressions = null;
                return false;
            }

            ISqlExpression joinAccumulator = sourceExpression;
            IEnumerable<ISqlExpression> expressions = Array.Empty<ISqlExpression>();
            var parameterExpressions = new List<Expressions.ParameterExpression>
            {
                sourceExpression.Parameter
            };

            foreach (var relation in relations)
            {
                var targetItemType = relation.Target;
                var targetQuerySourceExpression = new QuerySourceExpression(targetItemType);
                var targetParameterExpression = new Expressions.ParameterExpression(targetItemType, context.NextLambdaParameterName());
                parameterExpressions.Add(targetParameterExpression);
                var targetNamedSourceExpression = new NamedSourceExpression(targetItemType, targetQuerySourceExpression, targetParameterExpression);
                var targetExpressions = SelectAll(modelProvider, targetItemType, targetParameterExpression)
                    .Select(columnExpression => new RenameExpression(
                        columnExpression.Type,
                        new[] { relation.Property.Reflected, columnExpression.Member },
                        columnExpression))
                    .ToList();
                var onExpression = BuildJoinOnExpression(modelProvider, relation, sourceExpression.Parameter, targetParameterExpression);
                joinAccumulator = new JoinExpression(sourceExpression.ItemType, joinAccumulator, targetNamedSourceExpression, onExpression);

                expressions = expressions.Concat(targetExpressions);
            }

            joinExpression = (JoinExpression)joinAccumulator;
            relationExpressions = expressions.ToList();
            joinParameterExpressions = parameterExpressions;
            return true;

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