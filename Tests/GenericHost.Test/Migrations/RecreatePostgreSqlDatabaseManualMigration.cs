namespace SpaceEngineers.Core.GenericHost.Test.Migrations
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Attributes;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Settings;
    using DataAccess.Orm.Sql.Extensions;
    using DataAccess.Orm.Sql.Settings;
    using DataAccess.Orm.Transaction;
    using Microsoft.Extensions.Logging;
    using Npgsql;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Host.Migrations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations;

    [Dependent(typeof(InitialMigration))]
    [Component(EnLifestyle.Singleton)]
    internal class RecreatePostgreSqlDatabaseManualMigration : IManualMigration,
                                                               ICollectionResolvable<IManualMigration>
    {
        private const string CommandText = @"
create extension if not exists dblink;

drop database if exists ""{0}"" with (FORCE);
create database ""{0}"";
grant all privileges on database ""{0}"" to ""{1}""";

        private readonly ISettingsProvider<SqlDatabaseSettings> _sqlDatabaseSettingsProvider;
        private readonly ISettingsProvider<OrmSettings> _ormSettingsProvider;
        private readonly ILogger _logger;

        public RecreatePostgreSqlDatabaseManualMigration(
            ISettingsProvider<SqlDatabaseSettings> sqlDatabaseSettingsProvider,
            ISettingsProvider<OrmSettings> ormSettingsProvider,
            ILogger logger)
        {
            _sqlDatabaseSettingsProvider = sqlDatabaseSettingsProvider;
            _ormSettingsProvider = ormSettingsProvider;
            _logger = logger;
        }

        public string Name { get; } = ModelMigrationsExecutor.GetAutomaticMigrationName(-1);

        [SuppressMessage("Analysis", "CA2000", Justification = "IDbConnection will be disposed in outer scope by client")]
        public async Task ExecuteManualMigration(CancellationToken token)
        {
            var sqlDatabaseSettings = await _sqlDatabaseSettingsProvider
               .Get(token)
               .ConfigureAwait(false);

            var ormSettings = await _ormSettingsProvider
               .Get(token)
               .ConfigureAwait(false);

            var commandText = CommandText.Format(sqlDatabaseSettings.Database, sqlDatabaseSettings.Username);

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = sqlDatabaseSettings.Host,
                Port = sqlDatabaseSettings.Port,
                Database = "postgres",
                Username = sqlDatabaseSettings.Username,
                Password = sqlDatabaseSettings.Password
            };

            var npgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);

            using (var connection = new DatabaseConnection(npgSqlConnection))
            {
                await npgSqlConnection.OpenAsync(token).ConfigureAwait(false);

                _ = await connection
                   .InvokeScalar(commandText, ormSettings, _logger, token)
                   .ConfigureAwait(false);
            }

            NpgsqlConnection.ClearAllPools();
        }
    }
}