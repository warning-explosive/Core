namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Linq;

    /// <summary>
    /// InsertExpression
    /// </summary>
    public class InsertExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">ItemType</param>
        /// <param name="insertBehavior">Insert behavior</param>
        /// <param name="values">Values</param>
        public InsertExpression(
            Type itemType,
            EnInsertBehavior insertBehavior,
            IReadOnlyCollection<ValuesExpression> values)
        {
            ItemType = itemType;
            InsertBehavior = insertBehavior;
            Values = values.ToList();
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type ItemType { get; }

        /// <summary>
        /// Insert behavior
        /// </summary>
        public EnInsertBehavior InsertBehavior { get; }

        /// <summary>
        /// Values
        /// </summary>
        public IReadOnlyCollection<ValuesExpression> Values { get; }
    }
}