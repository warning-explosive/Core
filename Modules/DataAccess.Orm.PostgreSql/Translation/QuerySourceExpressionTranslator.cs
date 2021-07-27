namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using Linq.Abstractions;
    using Linq.Expressions;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class QuerySourceExpressionTranslator : IExpressionTranslator<QuerySourceExpression>
    {
        private readonly ISettingsProvider<PostgreSqlDatabaseSettings> _databaseSettings;

        public QuerySourceExpressionTranslator(ISettingsProvider<PostgreSqlDatabaseSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }

        public string Translate(QuerySourceExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(_databaseSettings.Get().Result.Schema);
            sb.Append('.');
            sb.Append('\"');
            sb.Append(expression.ItemType.Name);
            sb.Append('\"');

            return sb.ToString();
        }
    }
}