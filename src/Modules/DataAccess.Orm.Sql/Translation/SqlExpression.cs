namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Expressions;

    /// <summary>
    /// SqlExpression
    /// </summary>
    public class SqlExpression
    {
        internal SqlExpression(
            ISqlExpression expression,
            Func<Expression, IReadOnlyCollection<SqlCommandParameter>> commandParametersExtractor)
        {
            Expression = expression;
            CommandParametersExtractor = commandParametersExtractor;
        }

        /// <summary>
        /// Expression
        /// </summary>
        public ISqlExpression Expression { get; }

        /// <summary>
        /// CommandParametersExtractor
        /// </summary>
        public Func<Expression, IReadOnlyCollection<SqlCommandParameter>> CommandParametersExtractor { get; }
    }
}