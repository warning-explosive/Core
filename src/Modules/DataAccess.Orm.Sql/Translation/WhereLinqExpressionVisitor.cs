namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
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

                        if (itemType.IsPrimitive())
                        {
                            /*
                             * propagate primitive from source projection selector
                             */

                            var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());

                            ISqlExpression predicateExpression;

                            using (context.OpenParametersScope(parameterExpression))
                            {
                                visitor.Visit(methodCallExpression.Arguments[1]);
                                predicateExpression = context.SqlExpression;
                            }

                            var namedSourceExpression = new NamedSourceExpression(itemType, new ParenthesesExpression(sourceProjectionExpression), parameterExpression);
                            var sourceProjectionSelector = sourceProjectionExpression.Expressions.Single();
                            var replacedSourceProjectionSelector = ReplaceParameterSqlExpressionVisitor.Replace(sourceProjectionSelector, parameterExpression);
                            var expressions = new[] { replacedSourceProjectionSelector };
                            var replacedPredicateExpression = ReplaceParameterSqlExpressionVisitor.Replace(predicateExpression, replacedSourceProjectionSelector);
                            var filterExpression = new FilterExpression(itemType, replacedPredicateExpression);
                            var projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions, filterExpression, null);
                            context.Remember(projectionExpression);
                        }
                        else if (itemType.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
                        {
                            /*
                             * propagate relation from source projection selector
                             */

                            var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());

                            ISqlExpression predicateExpression;

                            using (context.OpenParametersScope(parameterExpression))
                            {
                                visitor.Visit(methodCallExpression.Arguments[1]);
                                predicateExpression = context.SqlExpression;
                            }

                            var namedSourceExpression = new NamedSourceExpression(itemType, new ParenthesesExpression(sourceProjectionExpression), parameterExpression);
                            var expressions = sourceProjectionExpression
                                .Expressions
                                .Select(sqlExpression => ReplaceParameterSqlExpressionVisitor.Replace(sqlExpression, parameterExpression))
                                .Select(sqlExpression =>
                                {
                                    return sqlExpression switch
                                    {
                                        ColumnExpression columnExpression => new ColumnExpression(columnExpression.Type, columnExpression.Member, parameterExpression),
                                        ColumnsChainExpression columnsChainExpression => (ISqlExpression)new ColumnsChainExpression(columnsChainExpression.Type, columnsChainExpression.Members, parameterExpression),
                                        RenameExpression renameExpression => new ColumnsChainExpression(renameExpression.Type, renameExpression.Members, parameterExpression),
                                        _ => throw new NotSupportedException(sqlExpression.GetType().FullName)
                                    };
                                })
                                .ToList();
                            var filterExpression = new FilterExpression(itemType, predicateExpression);
                            var projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions, filterExpression, null);
                            context.Remember(projectionExpression);
                        }
                        else if (sourceProjectionExpression.Expressions.OfType<RenameExpression>().Any())
                        {
                            /*
                             * propagate anonymous type renames
                             */

                            var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());

                            ISqlExpression predicateExpression;

                            using (context.OpenParametersScope(parameterExpression))
                            {
                                visitor.Visit(methodCallExpression.Arguments[1]);
                                predicateExpression = context.SqlExpression;
                            }

                            var namedSourceExpression = new NamedSourceExpression(itemType, new ParenthesesExpression(sourceProjectionExpression), parameterExpression);
                            var expressions = sourceProjectionExpression
                                .Expressions
                                .Select(sqlExpression => ReplaceParameterSqlExpressionVisitor.Replace(sqlExpression, parameterExpression))
                                .Select(sqlExpression =>
                                {
                                    return sqlExpression switch
                                    {
                                        ColumnExpression columnExpression => new ColumnExpression(columnExpression.Type, columnExpression.Member, parameterExpression),
                                        ColumnsChainExpression columnsChainExpression => (ISqlExpression)new ColumnsChainExpression(columnsChainExpression.Type, columnsChainExpression.Members, parameterExpression),
                                        RenameExpression renameExpression => new ColumnsChainExpression(renameExpression.Type, renameExpression.Members, parameterExpression),
                                        _ => throw new NotSupportedException(sqlExpression.GetType().FullName)
                                    };
                                })
                                .ToList();
                            var filterExpression = new FilterExpression(itemType, predicateExpression);
                            var projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions, filterExpression, null);
                            context.Remember(projectionExpression);
                        }
                        else
                        {
                            var parameterExpression = sourceProjectionExpression.Source switch
                            {
                                NamedSourceExpression namedSourceExpression => namedSourceExpression.Parameter,
                                _ => throw new NotSupportedException(sourceProjectionExpression.Source.GetType().FullName)
                            };

                            ISqlExpression predicateExpression;

                            using (context.OpenParametersScope(parameterExpression))
                            {
                                visitor.Visit(methodCallExpression.Arguments[1]);
                                predicateExpression = context.SqlExpression;
                            }

                            sourceProjectionExpression.FilterExpression = new FilterExpression(itemType, predicateExpression);
                            context.Remember(sourceProjectionExpression);
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

                        using (context.OpenParametersScope(parameterExpression))
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

                    case ProjectionExpression { Source: JoinExpression } sourceProjectionExpression:
                    {
                        /*
                         * apply filter to joined source
                         */

                        var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());

                        ISqlExpression predicateExpression;

                        using (context.OpenParametersScope(parameterExpression))
                        {
                            visitor.Visit(methodCallExpression.Arguments[1]);
                            predicateExpression = context.SqlExpression;
                        }

                        var namedSourceExpression = new NamedSourceExpression(itemType, new ParenthesesExpression(sourceProjectionExpression), parameterExpression);
                        var expressions = sourceProjectionExpression
                            .Expressions
                            .Select(sqlExpression => ReplaceParameterSqlExpressionVisitor.Replace(sqlExpression, parameterExpression))
                            .Select(sqlExpression =>
                            {
                                return sqlExpression switch
                                {
                                    ColumnExpression columnExpression => new ColumnExpression(columnExpression.Type, columnExpression.Member, parameterExpression),
                                    ColumnsChainExpression columnsChainExpression => (ISqlExpression)new ColumnsChainExpression(columnsChainExpression.Type, columnsChainExpression.Members, parameterExpression),
                                    RenameExpression renameExpression => new ColumnsChainExpression(renameExpression.Type, renameExpression.Members, parameterExpression),
                                    _ => throw new NotSupportedException(sqlExpression.GetType().FullName)
                                };
                            })
                            .ToList();
                        var filterExpression = new FilterExpression(itemType, predicateExpression);
                        var projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions, filterExpression, null);
                        context.Remember(projectionExpression);

                        break;
                    }

                    case QuerySourceExpression querySourceExpression:
                    {
                        /*
                         * filter raw table or view without projections
                         */

                        var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());
                        var namedSourceExpression = new NamedSourceExpression(itemType, querySourceExpression, parameterExpression);
                        var expressions = SelectLinqExpressionVisitor
                            .SelectAll(_modelProvider, itemType, parameterExpression)
                            .Cast<ISqlExpression>()
                            .ToList();

                        var relations = RelationsExpressionVisitor.ExtractRelations(_modelProvider, methodCallExpression.Arguments[1]);

                        ProjectionExpression projectionExpression;

                        if (SelectLinqExpressionVisitor.TryBuildJoinExpression(context, _modelProvider, namedSourceExpression, relations, out var joinExpression, out var relationExpressions, out var joinParameterExpressions))
                        {
                            ISqlExpression predicateExpression;

                            using (context.OpenParametersScope(joinParameterExpressions))
                            {
                                visitor.Visit(methodCallExpression.Arguments[1]);
                                predicateExpression = context.SqlExpression;
                            }

                            expressions = expressions.Concat(relationExpressions).ToList();
                            var filterExpression = new FilterExpression(itemType, predicateExpression);
                            projectionExpression = new ProjectionExpression(itemType, joinExpression, expressions, filterExpression, null);
                        }
                        else
                        {
                            ISqlExpression predicateExpression;

                            using (context.OpenParametersScope(parameterExpression))
                            {
                                visitor.Visit(methodCallExpression.Arguments[1]);
                                predicateExpression = context.SqlExpression;
                            }

                            var filterExpression = new FilterExpression(itemType, predicateExpression);
                            projectionExpression = new ProjectionExpression(itemType, namedSourceExpression, expressions, filterExpression, null);
                        }

                        context.Remember(projectionExpression);

                        break;
                    }

                    case UpdateExpression updateExpression:
                    {
                        /*
                         * filter update expression
                         */

                        var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());

                        ISqlExpression predicateExpression;

                        using (context.OpenParametersScope(parameterExpression))
                        {
                            visitor.Visit(methodCallExpression.Arguments[1]);
                            predicateExpression = context.SqlExpression;
                        }

                        // TODO: support join expression in update predicate and test it
                        var filterExpression = new FilterExpression(itemType, predicateExpression);
                        updateExpression.FilterExpression = filterExpression;
                        context.Remember(updateExpression);

                        break;
                    }

                    case DeleteExpression deleteExpression:
                    {
                        /*
                         * filter delete expression
                         */

                        var parameterExpression = new Expressions.ParameterExpression(itemType, context.NextLambdaParameterName());

                        ISqlExpression predicateExpression;

                        using (context.OpenParametersScope(parameterExpression))
                        {
                            visitor.Visit(methodCallExpression.Arguments[1]);
                            predicateExpression = context.SqlExpression;
                        }

                        // TODO: support join expression in delete predicate and test it
                        var filterExpression = new FilterExpression(itemType, predicateExpression);
                        deleteExpression.FilterExpression = filterExpression;
                        context.Remember(deleteExpression);

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
                    RenameExpression renameExpression => new RenameExpression(renameExpression.Type, renameExpression.Members, Replace(renameExpression.Source, replacement)),
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