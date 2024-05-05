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
    internal class RepositoryAllLinqExpressionVisitor : ILinqExpressionVisitor,
                                                        ICollectionResolvable<ILinqExpressionVisitor>
    {
        private readonly IModelProvider _modelProvider;

        public RepositoryAllLinqExpressionVisitor(IModelProvider modelProvider)
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

            if (method == LinqMethods.RepositoryAll())
            {
                var itemType = methodCallExpression.Type.ExtractQueryableItemType();

                if (context.IsOuterExpression() && !itemType.IsSqlView())
                {
                    var querySourceExpression = new QuerySourceExpression(itemType);

                    if (SelectLinqExpressionVisitor.BuildJoinExpression(context, _modelProvider, querySourceExpression) is { } joinProjectionExpression)
                    {
                        var projectionExpression = joinProjectionExpression;
                        context.Remember(projectionExpression);
                    }
                    else
                    {
                        var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());
                        var namedSourceExpression = new NamedSourceExpression(itemType, querySourceExpression, parameterExpression);
                        var expressions = SelectAll(_modelProvider, itemType, parameterExpression);
                        var projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions, null, null);
                        context.Remember(projectionExpression);
                    }
                }
                else
                {
                    var querySourceExpression = new QuerySourceExpression(itemType);

                    if (SelectLinqExpressionVisitor.BuildJoinExpression(context, _modelProvider, querySourceExpression) is { } projectionExpression)
                    {
                        context.Remember(projectionExpression);
                    }
                    else
                    {
                        context.Remember(querySourceExpression);
                    }
                }

                return true;
            }

            return false;
        }

        internal static IReadOnlyCollection<ISqlExpression> SelectAll(
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

                /*.return columns
                    .Select(column => column.BuildExpression(parameterExpression))

                    // TODO: do we need to select relations?
                    Concat(columns
                        .Where(column => column.IsRelation
                                         && !modelProvider.Tables[column.Relation.Target].IsMtmTable)
                        .SelectMany(column => modelProvider
                            .Columns(column.Relation.Target)
                            .Values
                            .Where(targetColumn => !targetColumn.IsMultipleRelation)
                            .Select(targetColumn => (column, targetColumn)))
                        .Select(pair =>
                        {
                            var (column, targetColumn) = pair;

                            var targetParameterExpression = new Expressions.ParameterExpression(targetColumn.Table.Type, parameterExpression.Name);
                            var columnExpression = targetColumn.BuildExpression(targetParameterExpression);
                            return (ISqlExpression)new RenameExpression(columnExpression.Type, $"{column.Relation.Property.Reflected.Name}_{columnExpression.Name}", columnExpression);
                        }))
                .ToList();*/
        }
    }
}