namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// ParenthesesExpression
    /// </summary>
    public class ParenthesesExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="source">Source</param>
        public ParenthesesExpression(ISqlExpression source)
        {
            if (source is not BinaryExpression
                && source is not ConditionalExpression
                && source is not JoinExpression
                && source is not JsonAttributeExpression
                && source is not NamedSourceExpression
                && source is not OrderByExpression
                && source is not ProjectionExpression
                && source is not QueryParameterExpression
                && source is not UnaryExpression)
            {
                throw new ArgumentException($"{nameof(ParenthesesExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            Source = source;
        }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; }
    }
}