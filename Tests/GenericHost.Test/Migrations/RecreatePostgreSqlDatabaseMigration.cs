namespace SpaceEngineers.Core.GenericHost.Test.Migrations
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Attributes;
    using CompositionRoot;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.Host.Abstractions;
    using DataAccess.Orm.Settings;
    using DataAccess.Orm.Sql.Extensions;
    using DataAccess.Orm.Sql.Host.Model;
    using DataAccess.Orm.Sql.Settings;
    using DataAccess.Orm.Transaction;
    using Microsoft.Extensions.Logging;
    using Npgsql;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations;

    [Component(EnLifestyle.Singleton)]
    [Before(typeof(InitialMigration))]
    internal class RecreatePostgreSqlDatabaseMigration : IMigration,
                                                         ICollectionResolvable<IMigration>
    {
        private const string CommandText = @"
create extension if not exists dblink;

drop database if exists ""{0}"" with (FORCE);
create database ""{0}"";
grant all privileges on database ""{0}"" to ""{1}""";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseImplementation _databaseImplementation;
        private readonly ISettingsProvider<SqlDatabaseSettings> _sqlDatabaseSettingsProvider;
        private readonly ISettingsProvider<OrmSettings> _ormSettingsProvider;
        private readonly IModelChangesExtractor _modelChangesExtractor;
        private readonly IMigrationsExecutor _migrationsExecutor;
        private readonly ILogger _logger;

        public RecreatePostgreSqlDatabaseMigration(
            IDependencyContainer dependencyContainer,
            IDatabaseImplementation databaseImplementation,
            ISettingsProvider<SqlDatabaseSettings> sqlDatabaseSettingsProvider,
            ISettingsProvider<OrmSettings> ormSettingsProvider,
            IModelChangesExtractor modelChangesExtractor,
            IMigrationsExecutor migrationsExecutor,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _databaseImplementation = databaseImplementation;
            _sqlDatabaseSettingsProvider = sqlDatabaseSettingsProvider;
            _ormSettingsProvider = ormSettingsProvider;
            _modelChangesExtractor = modelChangesExtractor;
            _migrationsExecutor = migrationsExecutor;
            _logger = logger;
        }

        public string Name { get; } = nameof(RecreatePostgreSqlDatabaseMigration);

        public bool ApplyEveryTime { get; } = true;

        [SuppressMessage("Analysis", "CA2000", Justification = "IDbConnection will be disposed in outer scope by client")]
        public async Task<string> Migrate(CancellationToken token)
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
                   .Execute(commandText, ormSettings, _logger, token)
                   .ConfigureAwait(false);
            }

            NpgsqlConnection.ClearAllPools();

            var migrations = new[]
            {
                new InitialMigration(_dependencyContainer, _databaseImplementation, _ormSettingsProvider, _modelChangesExtractor, _logger)
            };

            await _migrationsExecutor
               .Migrate(migrations, token)
               .ConfigureAwait(false);

            return commandText;
        }
    }
}