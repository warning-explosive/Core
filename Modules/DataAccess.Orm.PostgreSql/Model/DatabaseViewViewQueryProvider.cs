namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Api.Abstractions;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseViewViewQueryProvider : IViewQueryProvider<DatabaseView>
    {
        private static readonly string Query = $@"select
    substring(table_name, {{0}}) as ""{nameof(DatabaseView.Name)}"",
	view_definition as ""{nameof(DatabaseView.Query)}""
from INFORMATION_SCHEMA.views
where lower(substring(table_name, 0, {{0}})) == lower('{{1}}');";

        private readonly ISettingsProvider<PostgreSqlDatabaseSettings> _databaseSettings;

        public DatabaseViewViewQueryProvider(ISettingsProvider<PostgreSqlDatabaseSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }

        public async Task<string> GetQuery(CancellationToken token)
        {
            var databaseSettings = await _databaseSettings
                .Get(token)
                .ConfigureAwait(false);

            var prefix = databaseSettings.Schema + "_";

            return string.Format(Query, prefix.Length + 1, prefix);
        }
    }
}