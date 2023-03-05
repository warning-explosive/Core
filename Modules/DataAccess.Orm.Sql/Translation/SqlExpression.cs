namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Expressions;

    internal class SqlExpression
    {
        public SqlExpression(
            ISqlExpression expression,
            Func<Expression, IReadOnlyCollection<SqlCommandParameter>> commandParametersExtractor)
        {
            Expression = expression;
            CommandParametersExtractor = commandParametersExtractor;
        }

        public ISqlExpression Expression { get; }

        public Func<Expression, IReadOnlyCollection<SqlCommandParameter>> CommandParametersExtractor { get; }
    }
}