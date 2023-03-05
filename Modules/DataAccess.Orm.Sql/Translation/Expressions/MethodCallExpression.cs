namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// MethodCallExpression
    /// </summary>
    public class MethodCallExpression : ITypedSqlExpression,
                                        IApplicable<ColumnExpression>,
                                        IApplicable<ConditionalExpression>
    {
        private readonly List<ISqlExpression> _arguments;

        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="source">Source</param>
        /// <param name="arguments">Arguments</param>
        public MethodCallExpression(
            Type type,
            string name,
            ISqlExpression? source,
            IEnumerable<ISqlExpression> arguments)
        {
            Type = type;
            Name = name;
            Source = source;
            _arguments = arguments.ToList();
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression? Source { get; }

        /// <summary>
        /// Expression
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Arguments => _arguments;

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, ColumnExpression expression)
        {
            ApplyInternal(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression expression)
        {
            ApplyInternal(expression);
        }

        private void ApplyInternal(ISqlExpression expression)
        {
            _arguments.Add(expression);
        }

        #endregion
    }
}