namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// SetExpression
    /// </summary>
    public class SetExpression : ISqlExpression,
                                 IApplicable<UpdateExpression>,
                                 IApplicable<BinaryExpression>
    {
        private readonly List<ISqlExpression> _assignments;

        /// <summary> .cctor </summary>
        /// <param name="source">Source</param>
        /// <param name="assignments">Assignments</param>
        public SetExpression(
            ISqlExpression source,
            IEnumerable<ISqlExpression> assignments)
        {
            Source = source;
            _assignments = assignments.ToList();
        }

        internal SetExpression()
            : this(null!, new List<ISqlExpression>())
        {
        }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; private set; }

        /// <summary>
        /// Assignment
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Assignments => _assignments;

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, UpdateExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression expression)
        {
            ApplyAssignment(expression);
        }

        private void ApplySource(ISqlExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Source expression has already been set");
            }

            Source = expression;
        }

        private void ApplyAssignment(ISqlExpression expression)
        {
            _assignments.Add(expression);
        }

        #endregion
    }
}