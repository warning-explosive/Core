namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System.Collections.Generic;

    /// <summary>
    /// SetExpression
    /// </summary>
    public class SetExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="source">Source</param>
        /// <param name="assignments">Assignments</param>
        public SetExpression(
            UpdateExpression source,
            IReadOnlyCollection<BinaryExpression> assignments)
        {
            Source = source;
            Assignments = assignments;
        }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; }

        /// <summary>
        /// Assignment
        /// </summary>
        public IReadOnlyCollection<BinaryExpression> Assignments { get; }
    }
}