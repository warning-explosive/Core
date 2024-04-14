namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// RowsFetchLimitExpression
    /// </summary>
    public class RowsFetchLimitExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="source">ISqlExpression</param>
        /// <param name="rowsFetchLimit">Limit</param>
        public RowsFetchLimitExpression(ISqlExpression source, uint rowsFetchLimit)
        {
            if (source is not FilterExpression
                && source is not JoinExpression
                && source is not NamedSourceExpression
                && source is not ProjectionExpression)
            {
                throw new ArgumentException($"{nameof(RowsFetchLimitExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            RowsFetchLimit = rowsFetchLimit;
            Source = source;
        }

        /// <summary>
        /// Rows fetch limit
        /// </summary>
        public uint RowsFetchLimit { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; }
    }
}