namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Basics;
    using Basics.Primitives;
    using Expressions;
    using Linq;

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
        private ISqlExpression? _sqlExpression;

        private int _commandParameterIndex;
        private int _lambdaParameterIndex;

        /// <summary> .cctor </summary>
        internal TranslationContext()
        {
            _extractors = new Dictionary<string, Func<CommandParameterExtractionContext, ConstantExpression>>();
            _path = new Stack<Expression>();
            _sqlExpression = null;

            _commandParameterIndex = 0;
            _lambdaParameterIndex = 0;
        }

        /// <summary>
        /// ParameterExpression
        /// </summary>
        public Expressions.ParameterExpression? ParameterExpression { get; private set; }

        internal ISqlExpression SqlExpression => _sqlExpression ?? throw new InvalidOperationException("sql expression is empty");

        /// <summary> Remember </summary>
        /// <param name="expression">ISqlExpression</param>
        public void Remember(ISqlExpression expression)
        {
            _sqlExpression = expression;
        }

        /// <inheritdoc />
        public TranslationContext Clone()
        {
            return new TranslationContext
            {
                _extractors = _extractors,
                _path = _path,

                _commandParameterIndex = _commandParameterIndex,
                _lambdaParameterIndex = 0
            };
        }

        /// <inheritdoc />
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// OpenParameterScope
        /// </summary>
        /// <param name="parameter">ParameterExpression</param>
        /// <returns>Opened scope</returns>
        public IDisposable OpenParameterScope(Expressions.ParameterExpression parameter)
        {
            var previous = ParameterExpression;

            return Disposable.Create(parameter, Open, Close);

            void Open(Expressions.ParameterExpression parameterExpression)
            {
                ParameterExpression = parameterExpression;
            }

            void Close(Expressions.ParameterExpression parameterExpression)
            {
                ParameterExpression = previous;
            }
        }

        /// <summary>
        /// Gets next command parameter name
        /// </summary>
        /// <returns>Command parameter name</returns>
        public string NextCommandParameterName()
        {
            return CommandParameterFormat.Format(_commandParameterIndex++);
        }

        /// <summary>
        /// Gets next lambda parameter name
        /// </summary>
        /// <returns>Lambda parameter name</returns>
        public string NextLambdaParameterName()
        {
            return (_lambdaParameterIndex++).AlphabetIndex();
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

        internal bool IsOuterExpression()
        {
            if (_path.Count == 1)
            {
                return true;
            }

            if (_path.Count == 2
                && _path.Last() is System.Linq.Expressions.MethodCallExpression expression)
            {
                var method = expression.Method.GenericMethodDefinitionOrSelf();

                return method == LinqMethods.CachedExpression()
                       || method == LinqMethods.CachedInsertExpression()
                       || method == LinqMethods.CachedUpdateExpression()
                       || method == LinqMethods.CachedDeleteExpression()
                       || method == LinqMethods.WithDependencyContainer()
                       || method == LinqMethods.QueryableSingle()
                       || method == LinqMethods.QueryableSingleOrDefault()
                       || method == LinqMethods.QueryableFirst()
                       || method == LinqMethods.QueryableFirstOrDefault()
                       || method == LinqMethods.QueryableAny()
                       || method == LinqMethods.QueryableCount();
            }

            return false;
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