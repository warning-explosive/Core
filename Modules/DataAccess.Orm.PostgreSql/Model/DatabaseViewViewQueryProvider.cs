namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CrossCuttingConcerns.Api.Abstractions;
    using Settings;
    using Sql.Model;
    using Sql.Views;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseViewViewQueryProvider : ISqlViewQueryProvider<DatabaseView, Guid>
    {
        [SuppressMessage("Analysis", "CA1802", Justification = "interpolated string")]
        private static readonly string Query = $@"select
    TODO as ""{nameof(DatabaseView.PrimaryKey)}"",
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

            return Query.Format(prefix.Length + 1, prefix);
        }
    }
}