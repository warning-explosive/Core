namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
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
    internal class WhereLinqExpressionVisitor : ILinqExpressionVisitor,
                                                ICollectionResolvable<ILinqExpressionVisitor>
    {
        private readonly IModelProvider _modelProvider;

        public WhereLinqExpressionVisitor(IModelProvider modelProvider)
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

            if (method == LinqMethods.QueryableWhere()
                || method == LinqMethods.RepositoryUpdateWhere()
                || method == LinqMethods.RepositoryDeleteWhere())
            {
                var itemType = methodCallExpression.Type.ExtractQueryableItemType();

                visitor.Visit(methodCallExpression.Arguments[0]);
                var source = context.SqlExpression;

                var sourceItemType = SelectLinqExpressionVisitor.GetSourceItemType(source);
                var sourceSource = SelectLinqExpressionVisitor.GetSource(source);
                var sourceSourceItemType = sourceSource != null
                    ? SelectLinqExpressionVisitor.GetSourceItemType(sourceSource)
                    : null;

                switch (source)
                {
                    case ProjectionExpression sourceProjectionExpression when sourceSourceItemType != null && sourceItemType != sourceSourceItemType:
                    {
                        /*
                         * select made a projection
                         */

                        var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());

                        ISqlExpression predicateExpression;

                        using (context.OpenParameterScope(parameterExpression))
                        {
                            visitor.Visit(methodCallExpression.Arguments[1]);
                            predicateExpression = context.SqlExpression;
                        }

                        if (itemType.IsPrimitive())
                        {
                            /*
                             * propagate primitive source projection selector
                             */

                            var namedSourceExpression = new NamedSourceExpression(itemType, new ParenthesesExpression(sourceProjectionExpression), parameterExpression);
                            var sourceProjectionSelector = sourceProjectionExpression.Expressions.Single();
                            var replacedSourceProjectionSelector = ReplaceParameterSqlExpressionVisitor.Replace(sourceProjectionSelector, parameterExpression);
                            var expressions = new[] { replacedSourceProjectionSelector };
                            var replacedPredicateExpression = ReplaceParameterSqlExpressionVisitor.Replace(predicateExpression, replacedSourceProjectionSelector);
                            var filterExpression = new FilterExpression(itemType, replacedPredicateExpression);
                            var projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions, filterExpression, null);

                            if (SelectLinqExpressionVisitor.BuildJoinExpression(context, _modelProvider, projectionExpression) is { } joinProjectionExpression)
                            {
                                projectionExpression = joinProjectionExpression;
                            }

                            context.Remember(projectionExpression);
                        }
                        else
                        {
                            var namedSourceExpression = new NamedSourceExpression(itemType, new ParenthesesExpression(sourceProjectionExpression), parameterExpression);
                            var expressions = RepositoryAllLinqExpressionVisitor.SelectAll(_modelProvider, itemType, parameterExpression);
                            var filterExpression = new FilterExpression(itemType, predicateExpression);
                            var projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions, filterExpression, null);

                            if (SelectLinqExpressionVisitor.BuildJoinExpression(context, _modelProvider, projectionExpression) is { } joinProjectionExpression)
                            {
                                projectionExpression = joinProjectionExpression;
                            }

                            context.Remember(projectionExpression);
                        }

                        break;
                    }

                    case ProjectionExpression { Source: NamedSourceExpression namedSourceExpression } sourceProjectionExpression:
                    {
                        /*
                         * attach predicate to a projection selection
                         */

                        var parameterExpression = namedSourceExpression.Parameter;

                        ISqlExpression predicateExpression;

                        using (context.OpenParameterScope(parameterExpression))
                        {
                            visitor.Visit(methodCallExpression.Arguments[1]);
                            predicateExpression = context.SqlExpression;
                        }

                        if (sourceProjectionExpression.FilterExpression is { } sourceFilterExpression)
                        {
                            predicateExpression = new Expressions.BinaryExpression(typeof(bool), BinaryOperator.AndAlso, sourceFilterExpression.Predicate, predicateExpression);
                        }

                        var filterExpression = new FilterExpression(itemType, predicateExpression);
                        sourceProjectionExpression.FilterExpression = filterExpression;
                        context.Remember(sourceProjectionExpression);

                        break;
                    }

                    case ProjectionExpression { Source: JoinExpression joinExpression } sourceProjectionExpression:
                    {
                        /*
                         * apply filter to joined source
                         */

                        if (!TryGetJoinExpressionParameter(joinExpression, itemType, out var parameterExpression))
                        {
                            throw new InvalidOperationException("Unable to find suitable parameter expression");
                        }

                        ISqlExpression predicateExpression;

                        using (context.OpenParameterScope(parameterExpression))
                        {
                            visitor.Visit(methodCallExpression.Arguments[1]);
                            predicateExpression = context.SqlExpression;
                        }

                        // TODO: replace parameter
                        var filterExpression = new FilterExpression(itemType, predicateExpression);
                        sourceProjectionExpression.FilterExpression = filterExpression;
                        context.Remember(sourceProjectionExpression);

                        break;

                        static bool TryGetJoinExpressionParameter(
                            JoinExpression joinExpression,
                            Type itemType,
                            [NotNullWhen(true)] out Expressions.ParameterExpression? parameterExpression)
                        {
                            return TryGetJoinExpressionBranchParameter(joinExpression.LeftSource, itemType, out parameterExpression)
                                   || TryGetJoinExpressionBranchParameter(joinExpression.RightSource, itemType, out parameterExpression);
                        }

                        static bool TryGetJoinExpressionBranchParameter(
                            ISqlExpression sourceExpression,
                            Type itemType,
                            [NotNullWhen(true)] out Expressions.ParameterExpression? parameterExpression)
                        {
                            if (sourceExpression is JoinExpression leftSourceJoinExpression
                                && TryGetJoinExpressionParameter(leftSourceJoinExpression, itemType, out parameterExpression))
                            {
                                return true;
                            }

                            if (sourceExpression is NamedSourceExpression namedSourceExpression)
                            {
                                parameterExpression = namedSourceExpression.Parameter;
                                return true;
                            }

                            parameterExpression = null;
                            return false;
                        }
                    }

                    case QuerySourceExpression querySourceExpression:
                    {
                        /*
                         * filter raw table or view without projections
                         */

                        var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());

                        ISqlExpression predicateExpression;

                        using (context.OpenParameterScope(parameterExpression))
                        {
                            visitor.Visit(methodCallExpression.Arguments[1]);
                            predicateExpression = context.SqlExpression;
                        }

                        var namedSourceExpression = new NamedSourceExpression(itemType, querySourceExpression, parameterExpression);
                        var expressions = RepositoryAllLinqExpressionVisitor.SelectAll(_modelProvider, itemType, parameterExpression);
                        var filterExpression = new FilterExpression(itemType, predicateExpression);
                        var projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions, filterExpression, null);

                        if (SelectLinqExpressionVisitor.BuildJoinExpression(context, _modelProvider, projectionExpression) is { } joinProjectionExpression)
                        {
                            projectionExpression = joinProjectionExpression;
                        }

                        context.Remember(projectionExpression);

                        break;
                    }

                    case DeleteExpression deleteExpression:
                    {
                        /*
                         * filter delete expression
                         */

                        var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());

                        ISqlExpression predicateExpression;

                        using (context.OpenParameterScope(parameterExpression))
                        {
                            visitor.Visit(methodCallExpression.Arguments[1]);
                            predicateExpression = context.SqlExpression;
                        }

                        var filterExpression = new FilterExpression(itemType, predicateExpression);
                        deleteExpression.FilterExpression = filterExpression;
                        context.Remember(deleteExpression);

                        break;
                    }

                    case UpdateExpression updateExpression:
                    {
                        /*
                         * filter update expression
                         */

                        var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());

                        ISqlExpression predicateExpression;

                        using (context.OpenParameterScope(parameterExpression))
                        {
                            visitor.Visit(methodCallExpression.Arguments[1]);
                            predicateExpression = context.SqlExpression;
                        }

                        var filterExpression = new FilterExpression(itemType, predicateExpression);
                        updateExpression.FilterExpression = filterExpression;
                        context.Remember(updateExpression);

                        break;
                    }

                    default:
                    {
                        throw new NotSupportedException(source.GetType().FullName);
                    }
                }

                return true;
            }

            return false;
        }

        // TODO: private
        internal class ReplaceParameterSqlExpressionVisitor
        {
            internal static ISqlExpression Replace(ISqlExpression expression, ISqlExpression replacement)
            {
                return expression switch
                {
                    Expressions.BinaryExpression binaryExpression => new Expressions.BinaryExpression(
                        binaryExpression.Type,
                        binaryExpression.Operator,
                        Replace(binaryExpression.Left, replacement),
                        Replace(binaryExpression.Right, replacement)),
                    ColumnsChainExpression columnsChainExpression => new ColumnsChainExpression(
                        columnsChainExpression.Type,
                        columnsChainExpression.Members,
                        Replace(columnsChainExpression.Source, replacement)),
                    ColumnExpression columnExpression => new ColumnExpression(
                        columnExpression.Type,
                        columnExpression.Member,
                        Replace(columnExpression.Source, replacement)),
                    Expressions.ConditionalExpression conditionalExpression => new Expressions.ConditionalExpression(
                        conditionalExpression.Type,
                        Replace(conditionalExpression.When, replacement),
                        Replace(conditionalExpression.Then, replacement),
                        Replace(conditionalExpression.Else, replacement)),
                    JsonAttributeExpression jsonAttributeExpression => new JsonAttributeExpression(
                        jsonAttributeExpression.Type,
                        Replace(jsonAttributeExpression.Source, replacement),
                        Replace(jsonAttributeExpression.Accessor, replacement)),
                    Expressions.MethodCallExpression methodCallExpression => new Expressions.MethodCallExpression(
                        methodCallExpression.Type,
                        methodCallExpression.Name,
                        methodCallExpression.Source != null ? Replace(methodCallExpression.Source, replacement) : null,
                        methodCallExpression.Arguments.Select(argument => Replace(argument, replacement)).ToList()),
                    NullExpression nullExpression => nullExpression,
                    Expressions.ParameterExpression => replacement,
                    ParenthesesExpression parenthesesExpression => new ParenthesesExpression(Replace(parenthesesExpression.Source, replacement)),
                    QueryParameterExpression queryParameterExpression => queryParameterExpression,
                    Expressions.UnaryExpression unaryExpression => new Expressions.UnaryExpression(
                        unaryExpression.Type,
                        unaryExpression.Operator,
                        Replace(unaryExpression.Source, replacement)),
                    _ => throw new NotSupportedException(expression.GetType().Name)
                };
            }
        }
    }
}