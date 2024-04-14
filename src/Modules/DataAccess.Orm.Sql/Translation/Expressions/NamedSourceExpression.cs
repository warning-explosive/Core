namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// NamedSourceExpression
    /// </summary>
    public class NamedSourceExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source</param>
        /// <param name="parameter">Parameter</param>
        public NamedSourceExpression(
            Type type,
            ISqlExpression source,
            ParameterExpression parameter)
        {
            if (source is not FilterExpression
                && source is not JoinExpression
                && source is not OrderByExpression
                && source is not ParenthesesExpression
                && source is not ProjectionExpression
                && source is not QuerySourceExpression)
            {
                throw new ArgumentException($"{nameof(NamedSourceExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            Type = type;
            Source = source;
            Parameter = parameter;

            // TODO: remove forwarding - isn't obvious
            /*context.Apply(
                Source is FilterExpression filterExpression ? filterExpression.Source : Source,
                expression);*/
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

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