namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System.Collections.Generic;

    /// <summary>
    /// ValuesExpression
    /// </summary>
    public class ValuesExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="values">Values</param>
        public ValuesExpression(IReadOnlyCollection<QueryParameterExpression> values)
        {
            Values = values;
        }

        /// <summary>
        /// Values
        /// </summary>
        public IReadOnlyCollection<QueryParameterExpression> Values { get; }
    }
}