namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
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

        public async Task<string> Translate(QuerySourceExpression expression, int depth, CancellationToken token)
        {
            var databaseSettings = await _databaseSettings.Get(token).ConfigureAwait(false);

            var sb = new StringBuilder();

            sb.Append(databaseSettings.Schema);
            sb.Append('.');
            sb.Append('\"');
            sb.Append(expression.Type.Name);
            sb.Append('\"');

            return sb.ToString();
        }
    }
}