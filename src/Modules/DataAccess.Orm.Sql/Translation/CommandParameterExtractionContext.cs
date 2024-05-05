namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Model;

    /// <summary>
    /// CommandParameterExtractionContext
    /// </summary>
    public class CommandParameterExtractionContext
    {
        /// <summary> .cctor </summary>
        /// <param name="expression">Expression</param>
        /// <param name="parameterName">ParameterName</param>
        public CommandParameterExtractionContext(
            Expression expression,
            string parameterName)
        {
            Expression = expression;
            ParameterName = parameterName;
        }

        /// <summary>
        /// Expression
        /// </summary>
        public Expression Expression { get; }

        /// <summary>
        /// ParameterName
        /// </summary>
        public string ParameterName { get; }

        internal static Func<Expression, IReadOnlyCollection<SqlCommandParameter>> BuildCommandParametersExtractor(
            ILinqExpressionPreprocessorComposite preProcessor,
            IReadOnlyDictionary<string, Func<CommandParameterExtractionContext, ConstantExpression>> extractors)
        {
            return expression =>
            {
                var visitedExpression = preProcessor.Visit(expression);

                return extractors
                    .Select(pair =>
                    {
                        var (parameterName, extractor) = pair;
                        var context = new CommandParameterExtractionContext(visitedExpression, parameterName);
                        var constantExpression = extractor.Invoke(context);
                        return new SqlCommandParameter(parameterName, constantExpression.Value, constantExpression.Type);
                    })
                    .ToList();
            };
        }

        internal static Func<CommandParameterExtractionContext, ConstantExpression> GenerateCommandParameterExtractor(
            string parameterName,
            Expression[] path)
        {
            var expressionExtractor = new Func<CommandParameterExtractionContext, Expression>(context => context.Expression);

            for (var i = 0; i < path.Length - 1; i++)
            {
                var current = path[i];
                var next = path[i + 1];

                if (!TryFold(expressionExtractor, (current, next), out expressionExtractor))
                {
                    break;
                }
            }

            return context =>
            {
                var extracted = expressionExtractor(context);

                return extracted switch
                {
                    ConstantExpression constantExpression => constantExpression,
                    _ => throw new NotSupportedException($"Unable to extract command parameter from {extracted.GetType()}")
                };
            };
        }

        private static bool TryFold(
            Func<CommandParameterExtractionContext, Expression> acc,
            (Expression, Expression) pair,
            out Func<CommandParameterExtractionContext, Expression> extractor)
        {
            var (current, next) = pair;

            switch (current)
            {
                case IArgumentProvider argumentProvider:
                {
                    if (argumentProvider is MethodCallExpression methodCallExpression
                        && methodCallExpression.Object == next)
                    {
                        extractor = context => ((MethodCallExpression)acc(context)).Object;
                        return true;
                    }

                    for (var i = 0; i < argumentProvider.ArgumentCount; i++)
                    {
                        if (argumentProvider.GetArgument(i) == next)
                        {
                            extractor = context => ((IArgumentProvider)acc(context)).GetArgument(i);
                            return true;
                        }
                    }

                    break;
                }

                case UnaryExpression unaryExpression:
                {
                    if (unaryExpression.Operand == next)
                    {
                        extractor = context => ((UnaryExpression)acc(context)).Operand;
                        return true;
                    }

                    break;
                }

                case BinaryExpression binaryExpression:
                {
                    if (binaryExpression.Left == next)
                    {
                        extractor = context => ((BinaryExpression)acc(context)).Left;
                        return true;
                    }

                    if (binaryExpression.Right == next)
                    {
                        extractor = context => ((BinaryExpression)acc(context)).Right;
                        return true;
                    }

                    break;
                }

                case ConditionalExpression conditionalExpression:
                {
                    if (conditionalExpression.Test == next)
                    {
                        extractor = context => ((ConditionalExpression)acc(context)).Test;
                        return true;
                    }

                    if (conditionalExpression.IfTrue == next)
                    {
                        extractor = context => ((ConditionalExpression)acc(context)).IfTrue;
                        return true;
                    }

                    if (conditionalExpression.IfFalse == next)
                    {
                        extractor = context => ((ConditionalExpression)acc(context)).IfFalse;
                        return true;
                    }

                    break;
                }

                case LambdaExpression lambdaExpression:
                {
                    if (lambdaExpression.Body == next)
                    {
                        extractor = context => ((LambdaExpression)acc(context)).Body;
                        return true;
                    }

                    break;
                }

                case ConstantExpression constantExpression:
                {
                    if (next is MethodCallExpression methodCallExpression
                        && methodCallExpression.Method == InsertLinqExpressionVisitor.GetInsertValuesMethod
                        && methodCallExpression.Arguments[0] is ConstantExpression firstArgument
                        && firstArgument.Value is IModelProvider modelProvider)
                    {
                        extractor = context =>
                        {
                            var insertValuesMap = (IReadOnlyDictionary<string, ConstantExpression>)InsertLinqExpressionVisitor.GetInsertValuesMethod.Invoke(
                                null,
                                new[]
                                {
                                    modelProvider,
                                    ((ConstantExpression)acc(context)).Value
                                });

                            return insertValuesMap[context.ParameterName];
                        };

                        return true;
                    }

                    if (constantExpression.Value is IQueryable queryable
                        && queryable.Expression == next)
                    {
                        extractor = context => ((IQueryable)((ConstantExpression)acc(context)).Value).Expression;
                        return true;
                    }

                    break;
                }

                case MemberExpression memberExpression:
                {
                    if (memberExpression.Expression == next)
                    {
                        extractor = context => ((MemberExpression)acc(context)).Expression;
                        return true;
                    }

                    break;
                }
            }

            extractor = acc;
            return false;
        }
    }
}