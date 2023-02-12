namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using Expressions;

    internal class TranslatedSqlExpression : SqlExpression
    {
        public TranslatedSqlExpression(
            ISqlExpression expression,
            string commandText,
            Func<object, IReadOnlyCollection<SqlCommandParameter>> commandParametersExtractor)
            : base(expression, commandParametersExtractor)
        {
            CommandText = commandText;
        }

        public string CommandText { get; }
    }
}