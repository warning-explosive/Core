namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// ExplainExpression
    /// </summary>
    public class ExplainExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="source">Source</param>
        /// <param name="analyze">Analyze</param>
        public ExplainExpression(ISqlExpression source, bool analyze)
        {
            if (source is not FilterExpression
                && source is not JoinExpression
                && source is not NamedSourceExpression
                && source is not OrderByExpression
                && source is not ProjectionExpression
                && source is not RowsFetchLimitExpression)
            {
                throw new ArgumentException($"{nameof(ExplainExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            Analyze = analyze;
            Source = source;
        }

        /// <summary>
        /// Analyze
        /// </summary>
        public bool Analyze { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; }
    }
}