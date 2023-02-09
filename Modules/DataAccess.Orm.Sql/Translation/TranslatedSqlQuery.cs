namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Expressions;

    internal class TranslatedSqlQuery : SqlQuery
    {
        public TranslatedSqlQuery(
            ISqlExpression sqlExpression,
            string commandText,
            Func<Expression, IReadOnlyDictionary<string, string>> commandParametersExtractor)
            : base(sqlExpression, commandParametersExtractor)
        {
            CommandText = commandText;
        }

        public string CommandText { get; }
    }
}