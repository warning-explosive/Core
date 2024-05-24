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
                    var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());
                    var namedSourceExpression = new NamedSourceExpression(itemType, querySourceExpression, parameterExpression);
                    var expressions = SelectLinqExpressionVisitor
                        .SelectAll(_modelProvider, itemType, parameterExpression)
                        .Cast<ISqlExpression>()
                        .ToList();

                    IReadOnlyCollection<Relation> relations = itemType.IsMtmTable()
                        ? Array.Empty<Relation>()
                        : _modelProvider
                            .Columns(itemType)
                            .Select(it => it.Value)
                            .Where(column => column.IsRelation)
                            .Select(column => column.Relation!)
                            .ToList();

                    ProjectionExpression projectionExpression;

                    if (SelectLinqExpressionVisitor.TryBuildJoinExpression(context, _modelProvider, namedSourceExpression, relations, out var joinExpression, out var relationExpressions, out _))
                    {
                        expressions = expressions.Concat(relationExpressions).ToList();
                        projectionExpression = new ProjectionExpression(itemType, joinExpression, expressions, null, null);
                    }
                    else
                    {
                        projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions, null, null);
                    }

                    context.Remember(projectionExpression);
                }
                else
                {
                    var querySourceExpression = new QuerySourceExpression(itemType);
                    context.Remember(querySourceExpression);
                }

                return true;
            }

            return false;
        }
    }
}