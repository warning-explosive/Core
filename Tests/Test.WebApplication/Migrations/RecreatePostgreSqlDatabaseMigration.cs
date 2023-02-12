namespace SpaceEngineers.Core.Test.WebApplication.Migrations
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using CompositionRoot;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.Host.Abstractions;
    using DataAccess.Orm.Linq;
    using DataAccess.Orm.Sql.Host.Model;
    using DataAccess.Orm.Sql.Settings;
    using DataAccess.Orm.Sql.Translation;
    using Npgsql;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations;

    [Component(EnLifestyle.Singleton)]
    [Before(typeof(InitialMigration))]
    internal class RecreatePostgreSqlDatabaseMigration : IMigration,
                                                         ICollectionResolvable<IMigration>
    {
        private const string CommandText = @"create extension if not exists dblink;

drop database if exists ""{0}"" with (FORCE);
create database ""{0}"";
grant all privileges on database ""{0}"" to ""{1}"";";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<SqlDatabaseSettings> _sqlDatabaseSettingsProvider;
        private readonly IModelChangesExtractor _modelChangesExtractor;
        private readonly IModelChangeCommandBuilderComposite _commandBuilder;
        private readonly IMigrationsExecutor _migrationsExecutor;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public RecreatePostgreSqlDatabaseMigration(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<SqlDatabaseSettings> sqlDatabaseSettingsProvider,
            IModelChangesExtractor modelChangesExtractor,
            IModelChangeCommandBuilderComposite commandBuilder,
            IMigrationsExecutor migrationsExecutor,
            IDatabaseConnectionProvider connectionProvider)
        {
            _dependencyContainer = dependencyContainer;
            _sqlDatabaseSettingsProvider = sqlDatabaseSettingsProvider;
            _modelChangesExtractor = modelChangesExtractor;
            _commandBuilder = commandBuilder;
            _migrationsExecutor = migrationsExecutor;
            _connectionProvider = connectionProvider;
        }

        public string Name { get; } = nameof(RecreatePostgreSqlDatabaseMigration);

        public bool ApplyEveryTime { get; } = true;

        [SuppressMessage("Analysis", "CA2000", Justification = "IDbConnection will be disposed in outer scope by client")]
        public async Task<ICommand> Migrate(CancellationToken token)
        {
            var sqlDatabaseSettings = await _sqlDatabaseSettingsProvider
               .Get(token)
               .ConfigureAwait(false);

            var command = new SqlCommand(
                CommandText.Format(sqlDatabaseSettings.Database, sqlDatabaseSettings.Username),
                Array.Empty<SqlCommandParameter>());

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

                _ = await _connectionProvider
                   .Execute(connection, command, token)
                   .ConfigureAwait(false);
            }

            NpgsqlConnection.ClearAllPools();

            var migrations = new[]
            {
                new InitialMigration(_dependencyContainer, _modelChangesExtractor, _commandBuilder, _connectionProvider)
            };

            await _migrationsExecutor
               .Migrate(migrations, token)
               .ConfigureAwait(false);

            return command;
        }
    }
}