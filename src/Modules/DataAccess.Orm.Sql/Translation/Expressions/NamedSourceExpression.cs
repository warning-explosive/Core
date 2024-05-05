namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// NamedSourceExpression
    /// </summary>
    public class NamedSourceExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">ItemType</param>
        /// <param name="source">Source</param>
        /// <param name="parameter">Parameter</param>
        public NamedSourceExpression(
            Type itemType,
            ISqlExpression source,
            ParameterExpression parameter)
        {
            if (source is not JoinExpression
                && source is not OrderByExpression
                && source is not ParenthesesExpression
                && source is not ProjectionExpression
                && source is not QuerySourceExpression)
            {
                throw new ArgumentException($"{nameof(NamedSourceExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            ItemType = itemType;
            Source = source;
            Parameter = parameter;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type ItemType { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; }

        /// <summary>
        /// Parameter expression
        /// </summary>
        public ParameterExpression Parameter { get; }
    }
}