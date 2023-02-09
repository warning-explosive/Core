namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;
    using Basics.Primitives;
    using CompositionRoot;
    using Expressions;
    using Extensions;

    /// <summary>
    /// TranslationContext
    /// </summary>
    public class TranslationContext : ICloneable<TranslationContext>
    {
        internal const string QueryParameterFormat = "param_{0}";

        private Dictionary<string, Func<System.Linq.Expressions.Expression, object?>> _extractors;
        private Stack<System.Linq.Expressions.Expression> _path;

        private int _queryParameterIndex;

        private Dictionary<System.Linq.Expressions.ParameterExpression, ParameterExpression> _parameters;
        private Stack<ISqlExpression> _stack;

        private int _lambdaParameterIndex;
        private int _lambdaParametersCount;

        private ISqlExpression? _expression;

        /// <summary> .cctor </summary>
        internal TranslationContext()
        {
            _extractors = new Dictionary<string, Func<System.Linq.Expressions.Expression, object?>>();
            _path = new Stack<System.Linq.Expressions.Expression>();

            _queryParameterIndex = -1;

            _parameters = new Dictionary<System.Linq.Expressions.ParameterExpression, ParameterExpression>();
            _stack = new Stack<ISqlExpression>();

            _lambdaParameterIndex = -1;
            _lambdaParametersCount = 0;
        }

        /// <summary> .cctor </summary>
        /// <param name="context">TranslationContext</param>
        protected TranslationContext(TranslationContext context)
        {
            _extractors = context._extractors;
            _path = context._path;

            _queryParameterIndex = context._queryParameterIndex;

            _parameters = context._parameters;
            _stack = context._stack;

            _lambdaParameterIndex = context._lambdaParameterIndex;
            _lambdaParametersCount = context._lambdaParametersCount;
        }

        internal ISqlExpression? Expression
        {
            get => _expression;

            private set
            {
                ReverseLambdaParametersNames();
                _expression = value;
            }
        }

        internal ISqlExpression? Parent => _stack.TryPeek(out var parent) ? parent : default;

        /// <inheritdoc />
        public TranslationContext Clone()
        {
            return new TranslationContext
            {
                _extractors = _extractors,
                _path = _path,

                _queryParameterIndex = _queryParameterIndex,

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
        /// Gets next query parameter name
        /// </summary>
        /// <returns>Query parameter name</returns>
        public string NextQueryParameterName()
        {
            return QueryParameterFormat.Format(++_queryParameterIndex);
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

        internal void WithinScope<T>(T expression, Action? action = null)
            where T : class, ISqlExpression
        {
            using (Disposable.Create(_stack, Push, Pop))
            {
                action?.Invoke();
            }

            void Push(Stack<ISqlExpression> stack)
            {
                stack.Push(expression);
            }

            void Pop(Stack<ISqlExpression> stack)
            {
                var expr = (T)stack.Pop();

                if (_stack.TryPeek(out var outer))
                {
                    Apply(outer, expr);
                }
                else
                {
                    Expression = expr;
                }
            }
        }

        internal void WithoutScopeDuplication<T>(Func<T> sqlExpressionProducer, Action? action = null)
            where T : class, ISqlExpression
        {
            if (_stack.TryPeek(out var outer)
                && outer is T)
            {
                action?.Invoke();
            }
            else
            {
                WithinScope(sqlExpressionProducer(), action);
            }
        }

        [SuppressMessage("Analysis", "CA1822", Justification = "should be presented as instance method")]
        internal void WithinConditionalScope(
            Func<ISqlExpression?, bool> condition,
            Action<Action?> conditionalAction,
            Action? action = null)
        {
            if (condition(Parent))
            {
                conditionalAction(action);
            }
            else
            {
                action?.Invoke();
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

        internal Func<System.Linq.Expressions.Expression, IReadOnlyDictionary<string, string>> BuildCommandParametersExtractor(
            IDependencyContainer dependencyContainer,
            ILinqExpressionPreprocessorComposite preProcessor)
        {
            return expression =>
            {
                expression = preProcessor.Visit(expression);

                return _extractors.ToDictionary(
                    param => param.Key,
                    param => param.Value.Invoke(expression).QueryParameterSqlExpression(dependencyContainer));
            };
        }

        internal void CaptureCommandParameterExtractor(
            string name,
            Func<System.Linq.Expressions.Expression, object?>? extractor = null)
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

                _extractors[name] = expression => ((System.Linq.Expressions.ConstantExpression)expressionExtractor(expression)).Value;
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
                    case System.Linq.Expressions.IArgumentProvider methodCallExpression:
                    {
                        for (var i = 0; i < methodCallExpression.ArgumentCount; i++)
                        {
                            if (methodCallExpression.GetArgument(i) == next)
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
        }

        internal void PopPath(System.Linq.Expressions.Expression expression)
        {
            _ = _path.Pop();
        }
    }
}