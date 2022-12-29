namespace SpaceEngineers.Core.GenericHost.Test.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
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
    using DataAccess.Orm.Sql.Host.Migrations;
    using DataAccess.Orm.Sql.Host.Model;
    using DataAccess.Orm.Sql.Settings;
    using DataAccess.Orm.Transaction;
    using Microsoft.Extensions.Logging;
    using Npgsql;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    [Before(typeof(InitialMigration))]
    internal class CreateOrGetExistedPostgreSqlDatabaseMigration : IMigration,
                                                                   ICollectionResolvable<IMigration>
    {
        private const string CommandText = @"
create extension if not exists dblink;

create or replace function CreateOrGetExistedDatabase() returns boolean as
$BODY$
    declare isDatabaseExists boolean default false;

    begin
        select exists(select * from pg_catalog.pg_database where datname = '{0}') into isDatabaseExists;
    
        if
            isDatabaseExists
        then
            raise notice 'database already exists';
        else
            perform dblink_connect('host=localhost user=' || '{1}' || ' password=' || '{2}' || ' dbname=' || current_database());
            perform dblink_exec('create database ""{0}""');
            perform dblink_exec('grant all privileges on database ""{0}"" to ""{1}""');
        end if;
    
        return not isDatabaseExists;
    end
$BODY$
language plpgsql;

select CreateOrGetExistedDatabase();";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseImplementation _databaseImplementation;
        private readonly ISettingsProvider<SqlDatabaseSettings> _sqlDatabaseSettingsProvider;
        private readonly ISettingsProvider<OrmSettings> _ormSettingsProvider;
        private readonly IModelChangesExtractor _modelChangesExtractor;
        private readonly IModelChangeCommandBuilderComposite _commandBuilder;
        private readonly IMigrationsExecutor _migrationsExecutor;
        private readonly ILogger _logger;

        public CreateOrGetExistedPostgreSqlDatabaseMigration(
            IDependencyContainer dependencyContainer,
            IDatabaseImplementation databaseImplementation,
            ISettingsProvider<SqlDatabaseSettings> sqlDatabaseSettingsProvider,
            ISettingsProvider<OrmSettings> ormSettingsProvider,
            IModelChangesExtractor modelChangesExtractor,
            IModelChangeCommandBuilderComposite commandBuilder,
            IMigrationsExecutor migrationsExecutor,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _databaseImplementation = databaseImplementation;
            _sqlDatabaseSettingsProvider = sqlDatabaseSettingsProvider;
            _ormSettingsProvider = ormSettingsProvider;
            _modelChangesExtractor = modelChangesExtractor;
            _commandBuilder = commandBuilder;
            _migrationsExecutor = migrationsExecutor;
            _logger = logger;
        }

        public string Name { get; } = nameof(CreateOrGetExistedPostgreSqlDatabaseMigration);

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

            var commandText = CommandText.Format(
                sqlDatabaseSettings.Database,
                sqlDatabaseSettings.Username,
                sqlDatabaseSettings.Password);

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = sqlDatabaseSettings.Host,
                Port = sqlDatabaseSettings.Port,
                Database = "postgres",
                Username = sqlDatabaseSettings.Username,
                Password = sqlDatabaseSettings.Password
            };

            bool databaseWasCreated;

            var npgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);

            using (var connection = new DatabaseConnection(npgSqlConnection))
            {
                await npgSqlConnection.OpenAsync(token).ConfigureAwait(false);

                var dynamicValues = await connection
                   .Query(commandText, ormSettings, _logger, token)
                   .ConfigureAwait(false);

                databaseWasCreated = (dynamicValues.SingleOrDefault() as IDictionary<string, object?>)?.SingleOrDefault().Value is bool value
                    ? value
                    : throw new InvalidOperationException($"Unable to identify {sqlDatabaseSettings.Database} database state");
            }

            NpgsqlConnection.ClearAllPools();

            if (databaseWasCreated)
            {
                var migrations = new[]
                {
                    new InitialMigration(_dependencyContainer, _databaseImplementation, _ormSettingsProvider, _modelChangesExtractor, _commandBuilder, _logger)
                };

                await _migrationsExecutor
                   .Migrate(migrations, token)
                   .ConfigureAwait(false);
            }

            return commandText;
        }
    }
}