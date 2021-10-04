namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using Dapper;
    using Npgsql;
    using Orm.Model;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class CreateDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateDatabase>
    {
        private readonly ISettingsProvider<PostgreSqlDatabaseSettings> _settingsProvider;

        public CreateDatabaseModelChangeMigration(ISettingsProvider<PostgreSqlDatabaseSettings> settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task Migrate(CreateDatabase change, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var commandText = $"CREATE DATABASE \"{settings.Database}\"";

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = settings.Host,
                Port = settings.Port,
                Database = "postgres",
                Username = settings.Username,
                Password = settings.Password
            };

            using (var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString))
            {
                await connection
                    .OpenAsync(token)
                    .ConfigureAwait(false);

                await connection
                    .ExecuteAsync(commandText, token) // TODO: timeout
                    .ConfigureAwait(false);
            }
        }
    }
}