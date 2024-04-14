namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// JsonAttributeExpression
    /// </summary>
    public class JsonAttributeExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source</param>
        /// <param name="accessor">Accessor</param>
        public JsonAttributeExpression(
            Type type,
            ISqlExpression source,
            ISqlExpression accessor)
        {
            if (source is not ColumnExpression
                && source is not JsonAttributeExpression
                && source is not ParameterExpression
                && source is not ParenthesesExpression
                && source is not QueryParameterExpression)
            {
                throw new ArgumentException($"{nameof(JsonAttributeExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            Type = type;
            Source = source;
            Accessor = accessor;

            // TODO:
            throw new ArgumentException($"{nameof(JsonAttributeExpression)} doesn't support {accessor.GetType().Name} as {nameof(accessor)} argument");
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; }

        /// <summary>
        /// Json attribute accessor
        /// </summary>
        public ISqlExpression Accessor { get; }
    }
}