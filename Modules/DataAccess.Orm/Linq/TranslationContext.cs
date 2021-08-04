namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Basics;
    using Basics.Primitives;
    using Expressions;

    /// <summary>
    /// TranslationContext
    /// </summary>
    public class TranslationContext
    {
        internal const string QueryParameterFormat = "param_{0}";

        private readonly Stack<IIntermediateExpression> _stack;

        private int _queryParameterIndex;

        private IDictionary<Type, int> _lambdaParameterIndexes;

        /// <summary> .cctor </summary>
        internal TranslationContext()
        {
            _stack = new Stack<IIntermediateExpression>();

            _queryParameterIndex = 0;

            _lambdaParameterIndexes = new Dictionary<Type, int>();
        }

        internal IIntermediateExpression? Expression { get; private set; }

        /// <summary>
        /// Gets next query parameter name
        /// </summary>
        /// <returns>Query parameter name</returns>
        public string NextQueryParameterName()
        {
            return string.Format(QueryParameterFormat, _queryParameterIndex++);
        }

        /// <summary>
        /// Gets next lambda parameter name
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Lambda parameter name</returns>
        public string NextLambdaParameterName(Type type)
        {
            var index = _lambdaParameterIndexes.AddOrUpdate(type, _ => 0, (_, prev) => prev + 1);

            var ranks = GetRanks(index, 'z' - 'a' + 1)
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
            return _stack
                .OfType<ISubsequentIntermediateExpression>()
                .SelectMany(UnwrapSequence)
                .OfType<NamedSourceExpression>()
                .Select(expression => expression.Parameter)
                .OfType<ParameterExpression>()
                .FirstOrDefault(it => it.Type == type) ?? new ParameterExpression(type, NextLambdaParameterName(type));

            static IEnumerable<ISubsequentIntermediateExpression> UnwrapSequence(ISubsequentIntermediateExpression expression)
            {
                yield return expression;

                if (expression.Source is ISubsequentIntermediateExpression subsequentSource)
                {
                    foreach (var source in UnwrapSequence(subsequentSource))
                    {
                        yield return source;
                    }
                }
            }
        }

        internal void WithinExpressionScope<T>(T expression, Action? action = null)
            where T : class, IIntermediateExpression
        {
            using (Disposable.Create(_stack, Push, Pop))
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

                if (_stack.TryPeek(out var outer))
                {
                    Apply(expr, outer);
                }
                else
                {
                    Expression = expr;
                }
            }
        }

        internal void WithoutExpressionScopeDuplication<T>(Func<T> intermediateExpressionProducer, Action? action = null)
            where T : class, IIntermediateExpression
        {
            if (_stack.TryPeek(out var outer)
                && outer is T)
            {
                action?.Invoke();
            }
            else
            {
                WithinExpressionScope(intermediateExpressionProducer(), action);
            }
        }

        internal void WithinScope<T>(Action action)
            where T : class, IIntermediateExpression
        {
            if (_stack.TryPeek(out var outer)
                && outer is T)
            {
                action.Invoke();
            }
        }

        internal void Apply(IIntermediateExpression expression)
        {
            if (_stack.TryPeek(out var outer))
            {
                Apply(expression, outer);
            }
        }

        internal void Apply(IIntermediateExpression inner, IIntermediateExpression outer)
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