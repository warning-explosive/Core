namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// OrderByExpression
    /// </summary>
    public class OrderByExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="expressions">Order by expressions</param>
        public OrderByExpression(IReadOnlyCollection<OrderByExpressionExpression> expressions)
        {
            Expressions = expressions.ToList();
        }

        /// <summary>
        /// Order by expressions
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Expressions { get; }
    }
}