namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CrossCuttingConcerns.Api.Abstractions;
    using Dapper;
    using Npgsql;
    using Orm.Model;
    using Orm.Settings;
    using Settings;

    [Component(EnLifestyle.Scoped)]
    internal class CreateDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateDatabase>
    {
        private const string CommandFormat = @"create database ""{0}""";

        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly ISettingsProvider<PostgreSqlDatabaseSettings> _postgresSettingsProvider;

        public CreateDatabaseModelChangeMigration(
            ISettingsProvider<OrmSettings> settingsProvider,
            ISettingsProvider<PostgreSqlDatabaseSettings> postgresSettingsProvider)
        {
            _settingsProvider = settingsProvider;
            _postgresSettingsProvider = postgresSettingsProvider;
        }

        public async Task Migrate(CreateDatabase change, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var postgresSettings = await _postgresSettingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = postgresSettings.Host,
                Port = postgresSettings.Port,
                Database = "postgres",
                Username = postgresSettings.Username,
                Password = postgresSettings.Password
            };

            using (var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString))
            {
                await connection
                    .OpenAsync(token)
                    .ConfigureAwait(false);

                var command = new CommandDefinition(
                    CommandFormat.Format(postgresSettings.Database),
                    null,
                    null,
                    settings.QueryTimeout.Seconds,
                    CommandType.Text,
                    CommandFlags.Buffered,
                    token);

                await connection
                    .ExecuteAsync(command)
                    .ConfigureAwait(false);
            }
        }
    }
}