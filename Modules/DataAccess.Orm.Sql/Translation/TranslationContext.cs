namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using Api.Model;
    using Basics;
    using Basics.Primitives;
    using Expressions;
    using Model;
    using ParameterExpression = Expressions.ParameterExpression;

    /// <summary>
    /// TranslationContext
    /// </summary>
    public class TranslationContext : ICloneable<TranslationContext>
    {
        internal const string QueryParameterFormat = "param_{0}";

        private readonly Dictionary<System.Linq.Expressions.ParameterExpression, ParameterExpression> _parameters;

        private int _queryParameterIndex;
        private int _lambdaParameterIndex;
        private int _lambdaParametersCount;

        private IIntermediateExpression? _expression;

        /// <summary> .cctor </summary>
        internal TranslationContext()
        {
            _parameters = new Dictionary<System.Linq.Expressions.ParameterExpression, ParameterExpression>();

            Stack = new Stack<IIntermediateExpression>();

            _queryParameterIndex = -1;
            _lambdaParameterIndex = -1;
            _lambdaParametersCount = 0;
        }

        /// <summary> .cctor </summary>
        /// <param name="context">TranslationContext</param>
        protected TranslationContext(TranslationContext context)
        {
            _parameters = context._parameters;

            Stack = context.Stack;

            _queryParameterIndex = context._queryParameterIndex;
            _lambdaParameterIndex = context._lambdaParameterIndex;
            _lambdaParametersCount = context._lambdaParametersCount;
        }

        internal IIntermediateExpression? Expression
        {
            get => _expression;

            private set
            {
                ReverseLambdaParametersNames();
                _expression = value;
            }
        }

        internal IIntermediateExpression? Parent => Stack.TryPeek(out var parent) ? parent : default;

        private Stack<IIntermediateExpression> Stack { get; init; }

        /// <inheritdoc />
        public TranslationContext Clone()
        {
            var copy = new TranslationContext
            {
                Stack = new Stack<IIntermediateExpression>(Stack),

                _queryParameterIndex = _queryParameterIndex,
                _lambdaParameterIndex = _lambdaParameterIndex,
                _lambdaParametersCount = _lambdaParametersCount
            };

            return copy;
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
            where T : class, IIntermediateExpression
        {
            using (Disposable.Create(Stack, Push, Pop))
            {
                action?.Invoke();
            }

            void Push(Stack<IIntermediateExpression> stack)
            {
                stack.Push(expression);
            }

            void Pop(Stack<IIntermediateExpression> stack)
            {
                var expr = (T)stack.Pop();

                if (Stack.TryPeek(out var outer))
                {
                    Apply(outer, expr);
                }
                else
                {
                    Expression = expr;
                }
            }
        }

        internal void WithoutScopeDuplication<T>(Func<T> intermediateExpressionProducer, Action? action = null)
            where T : class, IIntermediateExpression
        {
            if (Stack.TryPeek(out var outer)
                && outer is T)
            {
                action?.Invoke();
            }
            else
            {
                WithinScope(intermediateExpressionProducer(), action);
            }
        }

        [SuppressMessage("Analysis", "CA1822", Justification = "should be presented as instance method")]
        internal void WithinConditionalScope(
            Func<IIntermediateExpression?, bool> condition,
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

        internal void Apply(IIntermediateExpression expression)
        {
            if (Stack.TryPeek(out var outer))
            {
                Apply(outer, expression);
            }
            else
            {
                throw new InvalidOperationException($"Could not apply {expression.GetType().Name}. There is no parent expression.");
            }
        }

        internal void Apply(IIntermediateExpression outer, IIntermediateExpression inner)
        {
            var service = typeof(IApplicable<>).MakeGenericType(inner.GetType());

            if (outer.IsInstanceOfType(service))
            {
                outer
                    .CallMethod(nameof(IApplicable<IIntermediateExpression>.Apply))
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
            return ExtractNamedSourceParameterExpression(Stack, type)
                ?? ExtractParameterExpression(_parameters, type)
                ?? NextParameterExpression(type);

            static ParameterExpression? ExtractNamedSourceParameterExpression(Stack<IIntermediateExpression> stack, Type type)
            {
                var intermediateExpression = stack
                   .FirstOrDefault(expression => expression
                        is NamedSourceExpression
                        or FilterExpression
                        or ProjectionExpression
                        or JoinExpression
                        or OrderByExpression);

                var namedSourceExpression = ExtractNamedSourceExpression(intermediateExpression, type);

                return namedSourceExpression?.Parameter as ParameterExpression;

                static NamedSourceExpression? ExtractNamedSourceExpression(
                    IIntermediateExpression? expression,
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
        internal LambdaExpression ExtractLambdaExpression(
            System.Linq.Expressions.MethodCallExpression node,
            Expression selector)
        {
            return new ExtractLambdaExpressionVisitor()
               .Extract(selector)
               .EnsureNotNull(() => new NotSupportedException($"method: {node.Method}"));
        }

        [SuppressMessage("Analysis", "CA1822", Justification = "should be presented as instance method")]
        internal IReadOnlyCollection<Relation> ExtractRelations(
            Type type,
            Expression node,
            IModelProvider modelProvider)
        {
            return type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                ? new ExtractRelationsExpressionVisitor(modelProvider).Extract(node)
                : Array.Empty<Relation>();
        }

        [SuppressMessage("Analysis", "CA1822", Justification = "should be presented as instance method")]
        internal ProjectionExpression? GetProjectionExpression(IIntermediateExpression intermediateExpression)
        {
            return ExtractProjectionExpression(intermediateExpression);

            static ProjectionExpression? ExtractProjectionExpression(IIntermediateExpression? expression)
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
    }
}