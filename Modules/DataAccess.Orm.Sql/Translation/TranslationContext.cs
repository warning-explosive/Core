namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;
    using Basics.Primitives;
    using Expressions;

    /// <summary>
    /// TranslationContext
    /// </summary>
    public class TranslationContext : ICloneable<TranslationContext>
    {
        /// <summary>
        /// CommandParameterFormat
        /// </summary>
        public const string CommandParameterFormat = "param_{0}";

        private Dictionary<string, Func<System.Linq.Expressions.Expression, System.Linq.Expressions.ConstantExpression>> _extractors;
        private Stack<System.Linq.Expressions.Expression> _path;

        private int _commandParameterIndex;

        private Dictionary<System.Linq.Expressions.ParameterExpression, ParameterExpression> _parameters;
        private Stack<ISqlExpression> _stack;

        private int _lambdaParameterIndex;
        private int _lambdaParametersCount;

        private ISqlExpression? _sqlSqlExpression;

        /// <summary> .cctor </summary>
        internal TranslationContext()
        {
            _extractors = new Dictionary<string, Func<System.Linq.Expressions.Expression, System.Linq.Expressions.ConstantExpression>>();
            _path = new Stack<System.Linq.Expressions.Expression>();

            _commandParameterIndex = -1;

            _parameters = new Dictionary<System.Linq.Expressions.ParameterExpression, ParameterExpression>();
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

        internal System.Linq.Expressions.Expression? Node => _path.TryPeek(out var node) ? node : default;

        /// <inheritdoc />
        public TranslationContext Clone()
        {
            return new TranslationContext
            {
                _extractors = _extractors,
                _path = _path,

                _commandParameterIndex = _commandParameterIndex,

                _parameters = new Dictionary<System.Linq.Expressions.ParameterExpression, ParameterExpression>(),
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

        internal void WithinScope<T>(T expression, Action action)
            where T : class, ISqlExpression
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
                var sqlExpression = (T)stack.Pop();

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

        internal void WithoutScopeDuplication<T>(Func<T> sqlExpressionProducer, Action action)
            where T : class, ISqlExpression
        {
            if (_stack.TryPeek(out var outer)
                && outer is T)
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
            var service = typeof(IApplicable<>).MakeGenericType(inner.GetType());

            if (outer.IsInstanceOfType(service))
            {
                outer
                    .CallMethod(nameof(IApplicable<ISqlExpression>.Apply))
                    .WithArgument(this)
                    .WithArgument(inner)
                    .Invoke();
            }
            else
            {
                throw new InvalidOperationException($"Could not apply {inner.GetType().Name} for {outer.GetType().Name}");
            }
        }

        internal ParameterExpression NextParameterExpression(Type type)
        {
            return new ParameterExpression(this, type);
        }

        internal ParameterExpression GetParameterExpression(System.Linq.Expressions.ParameterExpression parameterExpression)
        {
            return _parameters.GetOrAdd(parameterExpression, expression => GetParameterExpression(expression.Type));
        }

        internal ParameterExpression GetParameterExpression(Type type)
        {
            return ExtractNamedSourceParameterExpression(_stack, type)
                ?? ExtractParameterExpression(_parameters, type)
                ?? NextParameterExpression(type);

            static ParameterExpression? ExtractNamedSourceParameterExpression(Stack<ISqlExpression> stack, Type type)
            {
                var sqlExpression = stack
                   .FirstOrDefault(expression => expression
                        is NamedSourceExpression
                        or FilterExpression
                        or ProjectionExpression
                        or JoinExpression
                        or OrderByExpression);

                var namedSourceExpression = ExtractNamedSourceExpression(sqlExpression, type);

                return namedSourceExpression?.Parameter as ParameterExpression;

                static NamedSourceExpression? ExtractNamedSourceExpression(
                    ISqlExpression? expression,
                    Type type)
                {
                    switch (expression)
                    {
                        case NamedSourceExpression namedSourceExpression:
                            return namedSourceExpression.Type == type
                                ? namedSourceExpression
                                : ExtractNamedSourceExpression(namedSourceExpression.Source, type);
                        case FilterExpression filterExpression:
                            return ExtractNamedSourceExpression(filterExpression.Source, type);
                        case ProjectionExpression projectionExpression:
                            return ExtractNamedSourceExpression(projectionExpression.Source, type);
                        case JoinExpression joinExpression:
                            return ExtractNamedSourceExpression(joinExpression.LeftSource, type)
                                ?? ExtractNamedSourceExpression(joinExpression.RightSource, type);
                        case OrderByExpression orderByExpression:
                            return ExtractNamedSourceExpression(orderByExpression.Source, type);
                        default:
                            return default;
                    }
                }
            }

            static ParameterExpression? ExtractParameterExpression(
                IReadOnlyDictionary<System.Linq.Expressions.ParameterExpression, ParameterExpression> parameters,
                Type type)
            {
                return parameters.SingleOrDefault(it => it.Key.Type == type).Value;
            }
        }

        internal void ReverseLambdaParametersNames()
        {
            if (_lambdaParametersCount == 0)
            {
                _lambdaParametersCount = _lambdaParameterIndex + 1;
            }
        }

        [SuppressMessage("Analysis", "CA1822", Justification = "should be presented as instance method")]
        internal ProjectionExpression? GetProjectionExpression(ISqlExpression? sqlExpression)
        {
            return ExtractProjectionExpression(sqlExpression);

            static ProjectionExpression? ExtractProjectionExpression(ISqlExpression? expression)
            {
                switch (expression)
                {
                    case NamedSourceExpression namedSourceExpression:
                        return ExtractProjectionExpression(namedSourceExpression.Source);
                    case FilterExpression filterExpression:
                        return ExtractProjectionExpression(filterExpression.Source);
                    case ProjectionExpression projectionExpression:
                        return projectionExpression;
                    case JoinExpression:
                        throw new InvalidOperationException("Ambiguous reference to join expression source");
                    case OrderByExpression orderByExpression:
                        return ExtractProjectionExpression(orderByExpression.Source);
                    default:
                        return default;
                }
            }
        }

        internal Func<System.Linq.Expressions.Expression, IReadOnlyCollection<SqlCommandParameter>> BuildCommandParametersExtractor(
            ILinqExpressionPreprocessorComposite preProcessor)
        {
            return expression =>
            {
                var visitedExpression = preProcessor.Visit(expression);

                return _extractors
                    .Select(pair =>
                    {
                        var constantExpression = pair.Value.Invoke(visitedExpression);
                        return new SqlCommandParameter(pair.Key, constantExpression.Value, constantExpression.Type);
                    })
                    .ToList();
            };
        }

        internal void CaptureCommandParameterExtractor(
            string name,
            Func<System.Linq.Expressions.Expression, System.Linq.Expressions.ConstantExpression>? extractor = null)
        {
            if (extractor == null)
            {
                var expressionExtractor = new Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression>(expression => expression);

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

                _extractors[name] = expression =>
                {
                    var extracted = expressionExtractor(expression);

                    return extracted switch
                    {
                        System.Linq.Expressions.ConstantExpression constantExpression => constantExpression,
                        _ => throw new NotSupportedException($"Unable to extract command parameter from {extracted.GetType()}")
                    };
                };
            }
            else
            {
                _extractors[name] = extractor;
            }

            static bool TryFold(
                Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> acc,
                (System.Linq.Expressions.Expression, System.Linq.Expressions.Expression) pair,
                out Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> extractor)
            {
                var (current, next) = pair;

                switch (current)
                {
                    case System.Linq.Expressions.IArgumentProvider argumentProvider:
                    {
                        if (argumentProvider is System.Linq.Expressions.MethodCallExpression methodCallExpression
                            && methodCallExpression.Object == next)
                        {
                            extractor = expression => ((System.Linq.Expressions.MethodCallExpression)acc(expression)).Object;
                            return true;
                        }

                        for (var i = 0; i < argumentProvider.ArgumentCount; i++)
                        {
                            if (argumentProvider.GetArgument(i) == next)
                            {
                                extractor = expression => ((System.Linq.Expressions.IArgumentProvider)acc(expression)).GetArgument(i);
                                return true;
                            }
                        }

                        break;
                    }

                    case System.Linq.Expressions.UnaryExpression unaryExpression:
                    {
                        if (unaryExpression.Operand == next)
                        {
                            extractor = expression => ((System.Linq.Expressions.UnaryExpression)acc(expression)).Operand;
                            return true;
                        }

                        break;
                    }

                    case System.Linq.Expressions.BinaryExpression binaryExpression:
                    {
                        if (binaryExpression.Left == next)
                        {
                            extractor = expression => ((System.Linq.Expressions.BinaryExpression)acc(expression)).Left;
                            return true;
                        }

                        if (binaryExpression.Right == next)
                        {
                            extractor = expression => ((System.Linq.Expressions.BinaryExpression)acc(expression)).Right;
                            return true;
                        }

                        break;
                    }

                    case System.Linq.Expressions.ConditionalExpression conditionalExpression:
                    {
                        if (conditionalExpression.Test == next)
                        {
                            extractor = expression => ((System.Linq.Expressions.ConditionalExpression)acc(expression)).Test;
                            return true;
                        }

                        if (conditionalExpression.IfTrue == next)
                        {
                            extractor = expression => ((System.Linq.Expressions.ConditionalExpression)acc(expression)).IfTrue;
                            return true;
                        }

                        if (conditionalExpression.IfFalse == next)
                        {
                            extractor = expression => ((System.Linq.Expressions.ConditionalExpression)acc(expression)).IfFalse;
                            return true;
                        }

                        break;
                    }

                    case System.Linq.Expressions.LambdaExpression lambdaExpression:
                    {
                        if (lambdaExpression.Body == next)
                        {
                            extractor = expression => ((System.Linq.Expressions.LambdaExpression)acc(expression)).Body;
                            return true;
                        }

                        break;
                    }

                    case System.Linq.Expressions.ConstantExpression constantExpression:
                    {
                        if (constantExpression.Value is IQueryable queryable
                            && queryable.Expression == next)
                        {
                            extractor = expression => ((IQueryable)((System.Linq.Expressions.ConstantExpression)acc(expression)).Value).Expression;
                            return true;
                        }

                        break;
                    }

                    case System.Linq.Expressions.MemberExpression memberExpression:
                    {
                        if (memberExpression.Expression == next)
                        {
                            extractor = expression => ((System.Linq.Expressions.MemberExpression)acc(expression)).Expression;
                            return true;
                        }

                        break;
                    }
                }

                extractor = acc;
                return false;
            }
        }

        internal void PushPath(System.Linq.Expressions.Expression expression)
        {
            _path.Push(expression);

            Expression ??= expression;
        }

        internal void PopPath(System.Linq.Expressions.Expression expression)
        {
            _ = _path.Pop();
        }
    }
}