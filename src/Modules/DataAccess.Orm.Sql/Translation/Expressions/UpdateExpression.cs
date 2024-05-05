namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// UpdateExpression
    /// </summary>
    public class UpdateExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">ItemType</param>
        /// <param name="assignments">Assignments</param>
        /// <param name="filterExpression">FilterExpression</param>
        public UpdateExpression(
            Type itemType,
            IReadOnlyCollection<BinaryExpression> assignments,
            FilterExpression? filterExpression)
        {
            ItemType = itemType;
            FilterExpression = filterExpression;
            Assignments = assignments;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type ItemType { get; }

        /// <summary>
        /// Assignment
        /// </summary>
        public IReadOnlyCollection<BinaryExpression> Assignments { get; }

        /// <summary>
        /// FilterExpression
        /// </summary>
        public FilterExpression? FilterExpression { get; set; }
    }
}