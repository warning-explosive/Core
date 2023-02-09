namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Expressions;

    internal class SqlQuery
    {
        public SqlQuery(
            ISqlExpression sqlExpression,
            Func<Expression, IReadOnlyDictionary<string, string>> commandParametersExtractor)
        {
            SqlExpression = sqlExpression;
            CommandParametersExtractor = commandParametersExtractor;
        }

        public ISqlExpression SqlExpression { get; }

        public Func<Expression, IReadOnlyDictionary<string, string>> CommandParametersExtractor { get; }
    }
}