namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ProjectionExpression
    /// </summary>
    public class ProjectionExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">ItemType</param>
        /// <param name="source">Source expression</param>
        /// <param name="expressions">Expressions</param>
        /// <param name="filterExpression">FilterExpression</param>
        /// <param name="orderByExpression">OrderByExpression</param>
        public ProjectionExpression(
            Type itemType,
            ISqlExpression source,
            IReadOnlyCollection<ISqlExpression> expressions,
            FilterExpression? filterExpression,
            OrderByExpression? orderByExpression)
        {
            if (source is not NamedSourceExpression
                && source is not JoinExpression)
            {
                throw new ArgumentException($"{nameof(ProjectionExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            ItemType = itemType;
            Source = source;
            Expressions = expressions;
            FilterExpression = filterExpression;
            OrderByExpression = orderByExpression;
        }

        /// <summary>
        /// ItemType
        /// </summary>
        public Type ItemType { get; }

        /// <summary>
        /// Is projection takes distinct values
        /// </summary>
        public bool IsDistinct { get; set; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; }

        /// <summary>
        /// Expressions
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Expressions { get; }

        /// <summary>
        /// FilterExpression
        /// </summary>
        public FilterExpression? FilterExpression { get; set; }

        /// <summary>
        /// OrderByExpression
        /// </summary>
        public OrderByExpression? OrderByExpression { get; set; }
    }
}