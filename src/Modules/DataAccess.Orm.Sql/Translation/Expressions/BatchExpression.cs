namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System.Collections.Generic;

    /// <summary>
    /// BatchExpression
    /// </summary>
    public class BatchExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="expressions">Expressions</param>
        public BatchExpression(IReadOnlyCollection<ISqlExpression> expressions)
        {
            Expressions = expressions;
        }

        /// <summary>
        /// Expressions
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Expressions { get; }
    }
}