namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Orm.Transaction;

    /// <summary>
    /// InsertExpression
    /// </summary>
    public class InsertExpression : ISqlExpression,
                                    IApplicable<ValuesExpression>
    {
        private readonly List<ValuesExpression> _values;

        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="insertBehavior">Insert behavior</param>
        /// <param name="values">Values</param>
        public InsertExpression(
            Type type,
            EnInsertBehavior insertBehavior,
            IReadOnlyCollection<ValuesExpression> values)
        {
            Type = type;
            InsertBehavior = insertBehavior;

            _values = values.ToList();
        }

        internal InsertExpression(Type type, EnInsertBehavior insertBehavior)
            : this(type, insertBehavior, new List<ValuesExpression>())
        {
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Values
        /// </summary>
        public IReadOnlyCollection<ValuesExpression> Values => _values;

        /// <summary>
        /// Insert behavior
        /// </summary>
        public EnInsertBehavior InsertBehavior { get; }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, ValuesExpression expression)
        {
            _values.Add(expression);
        }

        #endregion
    }
}