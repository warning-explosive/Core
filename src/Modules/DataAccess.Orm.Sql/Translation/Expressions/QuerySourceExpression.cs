namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// QuerySourceExpression
    /// </summary>
    public class QuerySourceExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">ItemType</param>
        public QuerySourceExpression(Type itemType)
        {
            ItemType = itemType;
        }

        /// <summary>
        /// ItemType
        /// </summary>
        public Type ItemType { get; }
    }
}