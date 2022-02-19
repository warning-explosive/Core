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
    using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;
    using ParameterExpression = Expressions.ParameterExpression;

    /// <summary>
    /// TranslationContext
    /// </summary>
    public class TranslationContext : ICloneable<TranslationContext>
    {
        internal const string QueryParameterFormat = "param_{0}";

        private int _queryParameterIndex;
        private int _lambdaParameterIndex;
        private int _maxLambdaParameterIndex;

        /// <summary> .cctor </summary>
        internal TranslationContext()
        {
            _queryParameterIndex = 0;
            _lambdaParameterIndex = 0;
            Stack = new Stack<IIntermediateExpression>();
        }

        /// <summary> .cctor </summary>
        /// <param name="context">TranslationContext</param>
        internal TranslationContext(TranslationContext context)
        {
            _queryParameterIndex = context._queryParameterIndex;
            _lambdaParameterIndex = context._lambdaParameterIndex;
            Stack = context.Stack;
        }

        internal IIntermediateExpression? Expression { get; private set; }

        internal IIntermediateExpression? Parent => Stack.TryPeek(out var parent) ? parent : default;

        private Stack<IIntermediateExpression> Stack { get; init; }

        /// <inheritdoc />
        public TranslationContext Clone()
        {
            var copy = new TranslationContext
            {
                Stack = new Stack<IIntermediateExpression>(Stack),
                _queryParameterIndex = _queryParameterIndex,
                _lambdaParameterIndex = _lambdaParameterIndex
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
            return QueryParameterFormat.Format(_queryParameterIndex++);
        }

        /// <summary>
        /// Gets next lambda parameter name
        /// </summary>
        /// <returns>Lambda parameter name</returns>
        public Func<string> NextLambdaParameterName()
        {
            var lambdaParameterIndex = _lambdaParameterIndex;
            _lambdaParameterIndex++;

            return () =>
            {
                var ranks = GetRanks(_maxLambdaParameterIndex - lambdaParameterIndex - 1, 'z' - 'a' + 1)
                    .Select(rank => (char)('a' + rank))
                    .ToArray();

                return new string(ranks);
            };

            static IEnumerable<int> GetRanks(int index, int length)
            {
                var current = index;

                while (current >= length)
                {
                    yield return (current / length) - 1;
                    current = current % length;
                }

                yield return current;
            }
        }

        /// <summary>
        /// Reverses lambda parameters names
        /// </summary>
        public void ReverseLambdaParametersNames()
        {
            _maxLambdaParameterIndex = _lambdaParameterIndex;
        }

        /// <summary>
        /// Gets next parameter expression
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Parameter expression</returns>
        public ParameterExpression NextParameterExpression(Type type)
        {
            return new ParameterExpression(this, type);
        }

        /// <summary>
        /// Try get parameter expression
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Parameter expression was got successfully or not</returns>
        public ParameterExpression GetParameterExpression(Type type)
        {
            var intermediateExpression = Stack
                .FirstOrDefault(expression => expression is NamedSourceExpression or FilterExpression or ProjectionExpression or JoinExpression);

            var namedSourceExpression = ExtractNamedSourceExpression(intermediateExpression, type);

            var parameterExpression = namedSourceExpression?.Parameter as ParameterExpression;

            return parameterExpression ?? new ParameterExpression(this, type);

            static NamedSourceExpression? ExtractNamedSourceExpression(IIntermediateExpression? expression, Type type)
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
                    default:
                        return default;
                }
            }
        }

        internal void Push(IIntermediateExpression expression)
        {
            Stack.Push(expression);
            Expression = expression;
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

        [SuppressMessage("Analysis", "CA1822", Justification = "desired instance method")]
        internal void WithinConditionalScope(
            bool condition,
            Action<Action?> conditionalAction,
            Action? action = null)
        {
            if (condition)
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

        internal static IReadOnlyCollection<Relation> ExtractRelations(
            Type type,
            Expression node,
            IModelProvider modelProvider)
        {
            return type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                ? new ExtractRelationsExpressionVisitor(modelProvider).Extract(node)
                : Array.Empty<Relation>();
        }

        internal static LambdaExpression ExtractLambdaExpression(
            MethodCallExpression node,
            Expression selector)
        {
            return new ExtractLambdaExpressionVisitor()
                .Extract(selector)
                .EnsureNotNull(() => new NotSupportedException($"method: {node.Method}"));
        }
    }
}