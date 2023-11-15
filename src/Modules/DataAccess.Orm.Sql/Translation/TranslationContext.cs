namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;
    using Basics.Primitives;
    using Expressions;
    using Model;

    /// <summary>
    /// TranslationContext
    /// </summary>
    public class TranslationContext : ICloneable<TranslationContext>
    {
        /// <summary>
        /// CommandParameterFormat
        /// </summary>
        public const string CommandParameterFormat = "param_{0}";

        private Dictionary<string, Func<CommandParameterExtractorContext, string, System.Linq.Expressions.ConstantExpression>> _extractors;
        private Stack<System.Linq.Expressions.Expression> _path;

        private int _commandParameterIndex;

        private Stack<ISqlExpression> _stack;

        private int _lambdaParameterIndex;
        private int _lambdaParametersCount;

        private ISqlExpression? _sqlSqlExpression;

        /// <summary> .cctor </summary>
        internal TranslationContext()
        {
            _extractors = new Dictionary<string, Func<CommandParameterExtractorContext, string, System.Linq.Expressions.ConstantExpression>>();
            _path = new Stack<System.Linq.Expressions.Expression>();

            _commandParameterIndex = -1;

            _stack = new Stack<ISqlExpression>();

            _lambdaParameterIndex = -1;
            _lambdaParametersCount = 0;
        }

        internal System.Linq.Expressions.Expression? Expression { get; private set; }

        internal ISqlExpression? SqlExpression
        {
            get => _sqlSqlExpression;

            private set
            {
                ReverseLambdaParametersNames();
                _sqlSqlExpression = value;
            }
        }

        internal ISqlExpression? Parent => _stack.TryPeek(out var parent) ? parent : default;

        internal ISqlExpression? Outer => _stack
            .FirstOrDefault(expression => expression is NamedSourceExpression
                or FilterExpression
                or ProjectionExpression
                or JoinExpression
                or OrderByExpression);

        internal System.Linq.Expressions.Expression? Node => _path.TryPeek(out var node) ? node : default;

        /// <inheritdoc />
        public TranslationContext Clone()
        {
            return new TranslationContext
            {
                _extractors = _extractors,
                _path = _path,

                _commandParameterIndex = _commandParameterIndex,

                _stack = new Stack<ISqlExpression>(),

                _lambdaParameterIndex = -1,
                _lambdaParametersCount = 0
            };
        }

        /// <inheritdoc />
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Gets next command parameter name
        /// </summary>
        /// <returns>Command parameter name</returns>
        public string NextCommandParameterName()
        {
            return CommandParameterFormat.Format(++_commandParameterIndex);
        }

        /// <summary>
        /// Gets next lambda parameter name
        /// </summary>
        /// <returns>Lambda parameter name</returns>
        public Func<string> NextLambdaParameterName()
        {
            var capturedLambdaParameterIndex = ++_lambdaParameterIndex;

            return () =>
            {
                ReverseLambdaParametersNames();
                return (_lambdaParametersCount - capturedLambdaParameterIndex - 1).AlphabetIndex();
            };
        }

        internal void WithinScope(ISqlExpression expression, Action action)
        {
            using (Disposable.Create(_stack, Push, Pop))
            {
                action.Invoke();
            }

            void Push(Stack<ISqlExpression> stack)
            {
                stack.Push(expression);
            }

            void Pop(Stack<ISqlExpression> stack)
            {
                var sqlExpression = stack.Pop();

                if (_stack.TryPeek(out var outer))
                {
                    Apply(outer, sqlExpression);
                }
                else
                {
                    SqlExpression = sqlExpression;
                }
            }
        }

        internal void WithoutScopeDuplication<TExpression>(Func<TExpression> sqlExpressionProducer, Action action)
            where TExpression : class, ISqlExpression
        {
            if (_stack.TryPeek(out var outer)
                && outer is TExpression)
            {
                action.Invoke();
            }
            else
            {
                WithinScope(sqlExpressionProducer(), action);
            }
        }

        [SuppressMessage("Analysis", "CA1822", Justification = "should be presented as instance method")]
        internal void WithinConditionalScope(
            Func<ISqlExpression?, bool> condition,
            Action<Action> conditionalAction,
            Action action)
        {
            if (condition(Parent))
            {
                conditionalAction(action);
            }
            else
            {
                action.Invoke();
            }
        }

        internal void Apply(ISqlExpression expression)
        {
            if (_stack.TryPeek(out var outer))
            {
                Apply(outer, expression);
            }
            else
            {
                throw new InvalidOperationException($"Could not apply {expression.GetType().Name}. There is no parent expression.");
            }
        }

        internal void Apply(ISqlExpression outer, ISqlExpression inner)
        {
            outer
                .CallMethod(nameof(IApplicable<ISqlExpression>.Apply))
                .WithArgument(this)
                .WithArgument(inner)
                .Invoke();
        }

        internal void ReverseLambdaParametersNames()
        {
            if (_lambdaParametersCount == 0)
            {
                _lambdaParametersCount = _lambdaParameterIndex + 1;
            }
        }

        internal Func<System.Linq.Expressions.Expression, IReadOnlyCollection<SqlCommandParameter>> BuildCommandParametersExtractor(
            ILinqExpressionPreprocessorComposite preProcessor)
        {
            return expression =>
            {
                var context = new CommandParameterExtractorContext(preProcessor.Visit(expression));

                return _extractors
                    .Select(pair =>
                    {
                        var constantExpression = pair.Value.Invoke(context, pair.Key);
                        return new SqlCommandParameter(pair.Key, constantExpression.Value, constantExpression.Type);
                    })
                    .ToList();
            };
        }

        [SuppressMessage("Analysis", "CA1502", Justification = "complex infrastructural code")]
        internal void CaptureCommandParameterExtractor(
            string commandParameterName,
            Func<CommandParameterExtractorContext, string, System.Linq.Expressions.ConstantExpression>? extractor = null)
        {
            if (extractor == null)
            {
                var expressionExtractor = new Func<CommandParameterExtractorContext, string, System.Linq.Expressions.Expression>((context, _) => context.Expression);

                var path = _path.Reverse().ToArray();

                for (var i = 0; i < path.Length - 1; i++)
                {
                    var current = path[i];
                    var next = path[i + 1];

                    if (!TryFold(expressionExtractor, (current, next), out expressionExtractor))
                    {
                        break;
                    }
                }

                _extractors[commandParameterName] = (context, name) =>
                {
                    var extracted = expressionExtractor(context, name);

                    return extracted switch
                    {
                        System.Linq.Expressions.ConstantExpression constantExpression => constantExpression,
                        _ => throw new NotSupportedException($"Unable to extract command parameter from {extracted.GetType()}")
                    };
                };
            }
            else
            {
                _extractors[commandParameterName] = extractor;
            }

            static bool TryFold(
                Func<CommandParameterExtractorContext, string, System.Linq.Expressions.Expression> acc,
                (System.Linq.Expressions.Expression, System.Linq.Expressions.Expression) pair,
                out Func<CommandParameterExtractorContext, string, System.Linq.Expressions.Expression> extractor)
            {
                var (current, next) = pair;

                switch (current)
                {
                    case System.Linq.Expressions.IArgumentProvider argumentProvider:
                    {
                        if (argumentProvider is System.Linq.Expressions.MethodCallExpression methodCallExpression
                            && methodCallExpression.Object == next)
                        {
                            extractor = (context, name) => ((System.Linq.Expressions.MethodCallExpression)acc(context, name)).Object;
                            return true;
                        }

                        for (var i = 0; i < argumentProvider.ArgumentCount; i++)
                        {
                            if (argumentProvider.GetArgument(i) == next)
                            {
                                extractor = (context, name) => ((System.Linq.Expressions.IArgumentProvider)acc(context, name)).GetArgument(i);
                                return true;
                            }
                        }

                        break;
                    }

                    case System.Linq.Expressions.UnaryExpression unaryExpression:
                    {
                        if (unaryExpression.Operand == next)
                        {
                            extractor = (context, name) => ((System.Linq.Expressions.UnaryExpression)acc(context, name)).Operand;
                            return true;
                        }

                        break;
                    }

                    case System.Linq.Expressions.BinaryExpression binaryExpression:
                    {
                        if (binaryExpression.Left == next)
                        {
                            extractor = (context, name) => ((System.Linq.Expressions.BinaryExpression)acc(context, name)).Left;
                            return true;
                        }

                        if (binaryExpression.Right == next)
                        {
                            extractor = (context, name) => ((System.Linq.Expressions.BinaryExpression)acc(context, name)).Right;
                            return true;
                        }

                        break;
                    }

                    case System.Linq.Expressions.ConditionalExpression conditionalExpression:
                    {
                        if (conditionalExpression.Test == next)
                        {
                            extractor = (context, name) => ((System.Linq.Expressions.ConditionalExpression)acc(context, name)).Test;
                            return true;
                        }

                        if (conditionalExpression.IfTrue == next)
                        {
                            extractor = (context, name) => ((System.Linq.Expressions.ConditionalExpression)acc(context, name)).IfTrue;
                            return true;
                        }

                        if (conditionalExpression.IfFalse == next)
                        {
                            extractor = (context, name) => ((System.Linq.Expressions.ConditionalExpression)acc(context, name)).IfFalse;
                            return true;
                        }

                        break;
                    }

                    case System.Linq.Expressions.LambdaExpression lambdaExpression:
                    {
                        if (lambdaExpression.Body == next)
                        {
                            extractor = (context, name) => ((System.Linq.Expressions.LambdaExpression)acc(context, name)).Body;
                            return true;
                        }

                        break;
                    }

                    case System.Linq.Expressions.ConstantExpression constantExpression:
                    {
                        if (next is System.Linq.Expressions.MethodCallExpression methodCallExpression
                            && methodCallExpression.Method == TranslationExpressionVisitor.GetInsertValuesMethod
                            && methodCallExpression.Arguments[0] is System.Linq.Expressions.ConstantExpression firstArgument
                            && firstArgument.Value is IModelProvider modelProvider)
                        {
                            extractor = (context, name) =>
                            {
                                var insertValuesMap = context.GetOrAdd(
                                    TranslationExpressionVisitor.GetInsertValuesMethod.Name,
                                    () => (IReadOnlyDictionary<string, System.Linq.Expressions.ConstantExpression>)TranslationExpressionVisitor.GetInsertValuesMethod.Invoke(
                                            null,
                                            new[]
                                            {
                                                modelProvider,
                                                ((System.Linq.Expressions.ConstantExpression)acc(context, name)).Value
                                            }));

                                return insertValuesMap[name];
                            };

                            return true;
                        }

                        if (constantExpression.Value is IQueryable queryable
                            && queryable.Expression == next)
                        {
                            extractor = (context, name) => ((IQueryable)((System.Linq.Expressions.ConstantExpression)acc(context, name)).Value).Expression;
                            return true;
                        }

                        break;
                    }

                    case System.Linq.Expressions.MemberExpression memberExpression:
                    {
                        if (memberExpression.Expression == next)
                        {
                            extractor = (context, name) => ((System.Linq.Expressions.MemberExpression)acc(context, name)).Expression;
                            return true;
                        }

                        break;
                    }
                }

                extractor = acc;
                return false;
            }
        }

        internal DisposableAction<System.Linq.Expressions.Expression> WithinPathScope(
            System.Linq.Expressions.Expression expression)
        {
            return Disposable.Create(expression, PushPath, PopPath);
        }

        private void PushPath(System.Linq.Expressions.Expression expression)
        {
            _path.Push(expression);

            Expression ??= expression;
        }

        private void PopPath(System.Linq.Expressions.Expression expression)
        {
            _ = _path.Pop();
        }
    }
}