namespace SpaceEngineers.Core.GenericHost.Test.Migrations
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Attributes;
    using CompositionRoot;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.Host.Abstractions;
    using DataAccess.Orm.Linq;
    using DataAccess.Orm.Sql.Host.Migrations;
    using DataAccess.Orm.Sql.Host.Model;
    using DataAccess.Orm.Sql.Settings;
    using DataAccess.Orm.Sql.Translation;
    using Npgsql;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    [Before(typeof(InitialMigration))]
    internal class CreateOrGetExistedPostgreSqlDatabaseMigration : IMigration,
                                                                   ICollectionResolvable<IMigration>
    {
        private const string CommandText = @"create extension if not exists dblink;

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
        private readonly ISettingsProvider<SqlDatabaseSettings> _sqlDatabaseSettingsProvider;
        private readonly IModelChangesExtractor _modelChangesExtractor;
        private readonly IModelChangeCommandBuilderComposite _commandBuilder;
        private readonly IMigrationsExecutor _migrationsExecutor;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public CreateOrGetExistedPostgreSqlDatabaseMigration(
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

        public string Name { get; } = nameof(CreateOrGetExistedPostgreSqlDatabaseMigration);

        public bool ApplyEveryTime { get; } = true;

        [SuppressMessage("Analysis", "CA2000", Justification = "IDbConnection will be disposed in outer scope by client")]
        public async Task<ICommand> InvokeCommand(CancellationToken token)
        {
            var sqlDatabaseSettings = await _sqlDatabaseSettingsProvider
               .Get(token)
               .ConfigureAwait(false);

            var command = new SqlCommand(
                CommandText.Format(sqlDatabaseSettings.Database, sqlDatabaseSettings.Username, sqlDatabaseSettings.Password),
                Array.Empty<SqlCommandParameter>());

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

            try
            {
                await npgSqlConnection.OpenAsync(token).ConfigureAwait(false);

                databaseWasCreated = await _connectionProvider
                    .ExecuteScalar<bool>(npgSqlConnection, command, token)
                    .ConfigureAwait(false);
            }
            finally
            {
                npgSqlConnection.Dispose();
            }

            NpgsqlConnection.ClearPool(npgSqlConnection);

            while (true)
            {
                var doesDatabaseExist = await _dependencyContainer
                    .Resolve<IDatabaseConnectionProvider>()
                    .DoesDatabaseExist(token)
                    .ConfigureAwait(false);

                if (!doesDatabaseExist)
                {
                    await Task
                        .Delay(TimeSpan.FromMilliseconds(100), token)
                        .ConfigureAwait(false);
                }
                else
                {
                    break;
                }
            }

            if (databaseWasCreated)
            {
                var migrations = new[]
                {
                    new InitialMigration(_dependencyContainer, _modelChangesExtractor, _commandBuilder, _connectionProvider)
                };

                await _migrationsExecutor
                   .Migrate(migrations, token)
                   .ConfigureAwait(false);
            }

            return command;
        }
    }
}