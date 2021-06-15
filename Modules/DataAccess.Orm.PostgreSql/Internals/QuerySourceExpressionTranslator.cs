namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Internals
{
    using System.Text;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using DataAccess.PostgreSql.Settings;
    using Linq.Expressions;
    using SettingsManager.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class QuerySourceExpressionTranslator : IExpressionTranslator<QuerySourceExpression>
    {
        private readonly ISettingsManager<PostgreSqlDatabaseSettings> _postgreSqlSettingsProvider;

        public QuerySourceExpressionTranslator(ISettingsManager<PostgreSqlDatabaseSettings> postgreSqlSettingsProvider)
        {
            _postgreSqlSettingsProvider = postgreSqlSettingsProvider;
        }

        public string Translate(QuerySourceExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(_postgreSqlSettingsProvider.Get().Result.Schema);
            sb.Append('.');
            sb.Append('\"');
            sb.Append(expression.ItemType.Name);
            sb.Append('\"');

            return sb.ToString();
        }
    }
}