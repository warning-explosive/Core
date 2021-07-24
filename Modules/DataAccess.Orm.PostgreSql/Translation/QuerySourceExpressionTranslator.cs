namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.PostgreSql.Settings;
    using Linq.Abstractions;
    using Linq.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class QuerySourceExpressionTranslator : IExpressionTranslator<QuerySourceExpression>
    {
        private readonly ISettingsProvider<PostgreSqlDatabaseSettings> _postgreSqlSettings;

        public QuerySourceExpressionTranslator(ISettingsProvider<PostgreSqlDatabaseSettings> postgreSqlSettings)
        {
            _postgreSqlSettings = postgreSqlSettings;
        }

        public string Translate(QuerySourceExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(_postgreSqlSettings.Get().Result.Schema);
            sb.Append('.');
            sb.Append('\"');
            sb.Append(expression.ItemType.Name);
            sb.Append('\"');

            return sb.ToString();
        }
    }
}