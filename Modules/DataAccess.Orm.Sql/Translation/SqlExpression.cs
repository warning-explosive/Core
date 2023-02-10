namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using Expressions;

    internal class SqlExpression
    {
        // TODO: check creations
        public SqlExpression(
            ISqlExpression expression,
            Func<object, IReadOnlyCollection<SqlCommandParameter>> commandParametersExtractor)
        {
            Expression = expression;
            CommandParametersExtractor = commandParametersExtractor;
        }

        public ISqlExpression Expression { get; }

        public Func<object, IReadOnlyCollection<SqlCommandParameter>> CommandParametersExtractor { get; }
    }
}