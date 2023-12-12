namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
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

        private Dictionary<string, Func<CommandParameterExtractionContext, ConstantExpression>> _extractors;
        private Stack<Expression> _path;

        private int _commandParameterIndex;

        private Stack<ISqlExpression> _stack;

        private int _lambdaParameterIndex;
        private int _lambdaParametersCount;

        private ISqlExpression? _sqlSqlExpression;

        /// <summary> .cctor </summary>
        internal TranslationContext()
        {
            _extractors = new Dictionary<string, Func<CommandParameterExtractionContext, ConstantExpression>>();
            _path = new Stack<Expression>();

            _commandParameterIndex = -1;

            _stack = new Stack<ISqlExpression>();

            _lambdaParameterIndex = -1;
            _lambdaParametersCount = 0;
        }

        internal ISqlExpression? SqlExpression
        {
            get => _sqlSqlExpression;

            private set
            {
                ReverseLambdaParametersNames();
                _sqlSqlExpression = value;
            }
        }

        internal ISqlExpression? Outer => _stack
        internal Expression OriginalExpression => _path.Last();

            .FirstOrDefault(expression => expression is NamedSourceExpression
                or FilterExpression
                or ProjectionExpression
                or JoinExpression
                or OrderByExpression);

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

        internal void ReverseLambdaParametersNames()
        {
            if (_lambdaParametersCount == 0)
            {
                _lambdaParametersCount = _lambdaParameterIndex + 1;
            }
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
            _ = _stack.TryPeek(out var outer);

            if (condition(outer ?? default))
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

        internal Func<Expression, IReadOnlyCollection<SqlCommandParameter>> BuildCommandParametersExtractor(
            ILinqExpressionPreprocessorComposite preprocessor)
        {
            return CommandParameterExtractionContext.BuildCommandParametersExtractor(preprocessor, _extractors);
        }

        internal void CaptureCommandParameterExtractor(
            string parameterName,
            Func<CommandParameterExtractionContext, ConstantExpression>? extractor)
        {
            if (_extractors.ContainsKey(parameterName))
            {
                throw new InvalidOperationException($"command parameter {parameterName} have already been captured");
            }

            _extractors[parameterName] = extractor ?? CommandParameterExtractionContext.GenerateCommandParameterExtractor(parameterName, _path.Reverse().ToArray());
        }

        internal DisposableAction<Expression> WithinPathScope(
            Expression expression)
        {
            return Disposable.Create(expression, PushPath, PopPath);
        }

        private void PushPath(Expression expression)
        {
            _path.Push(expression);
        }

        private void PopPath(Expression expression)
        {
            _ = _path.Pop();
        }
    }
}