namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// DeleteExpression
    /// </summary>
    public class DeleteExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">ItemType</param>
        /// <param name="filterExpression">FilterExpression</param>
        public DeleteExpression(
            Type itemType,
            FilterExpression? filterExpression)
        {
            ItemType = itemType;
            FilterExpression = filterExpression;
        }

        /// <summary>
        /// ItemType
        /// </summary>
        public Type ItemType { get; }

        /// <summary>
        /// FilterExpression
        /// </summary>
        public FilterExpression? FilterExpression { get; set; }
    }
}