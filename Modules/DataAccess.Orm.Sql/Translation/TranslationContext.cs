namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Basics.Primitives;
    using Expressions;

    /// <summary>
    /// TranslationContext
    /// </summary>
    public class TranslationContext : ICloneable<TranslationContext>
    {
        internal const string QueryParameterFormat = "param_{0}";

        private int _queryParameterIndex;
        private int _lambdaParameterIndex;

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
        public string NextLambdaParameterName()
        {
            var ranks = GetRanks(_lambdaParameterIndex++, 'z' - 'a' + 1)
                .Select(rank => (char)('a' + rank))
                .ToArray();

            return new string(ranks);

            static IEnumerable<int> GetRanks(int index, int delta)
            {
                var current = index;

                while (current >= delta)
                {
                    yield return (current / delta) - 1;
                    current = current % delta;
                }

                yield return current;
            }
        }

        /// <summary>
        /// Try get parameter expression
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Parameter expression was got successfully or not</returns>
        public ParameterExpression GetParameterExpression(Type type)
        {
            var subsequentIntermediateExpression = Stack
                .OfType<ISubsequentIntermediateExpression>()
                .FirstOrDefault();

            ParameterExpression? parameter = null;

            if (subsequentIntermediateExpression != null)
            {
                parameter = FlattenSequence(subsequentIntermediateExpression)
                    .OfType<NamedSourceExpression>()
                    .Select(expression => expression.Parameter)
                    .OfType<ParameterExpression>()
                    .FirstOrDefault();
            }

            return parameter ?? new ParameterExpression(this, type);

            static IEnumerable<ISubsequentIntermediateExpression> FlattenSequence(ISubsequentIntermediateExpression expression)
            {
                yield return expression;

                if (expression.Source is ISubsequentIntermediateExpression subsequentSource)
                {
                    foreach (var source in FlattenSequence(subsequentSource))
                    {
                        yield return source;
                    }
                }
            }
        }

        internal void WithinScope<T>(Action action)
            where T : class, IIntermediateExpression
        {
            if (Stack.TryPeek(out var outer)
                && outer is T)
            {
                action.Invoke();
            }
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
                    Apply(expr, outer);
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

        internal void Apply(IIntermediateExpression expression)
        {
            if (Stack.TryPeek(out var outer))
            {
                Apply(expression, outer);
            }
        }

        private void Apply(IIntermediateExpression inner, IIntermediateExpression outer)
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
    }
}