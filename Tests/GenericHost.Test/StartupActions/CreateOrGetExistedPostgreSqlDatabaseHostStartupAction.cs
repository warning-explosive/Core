namespace SpaceEngineers.Core.GenericHost.Test.StartupActions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Basics;
    using Basics.Attributes;
    using CompositionRoot;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.Sql.Settings;
    using DataAccess.Orm.Sql.Translation;
    using Npgsql;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericEndpoint.DataAccess.Host.StartupActions;

    [Component(EnLifestyle.Singleton)]
    [Before(typeof(UpgradeDatabaseHostStartupAction))]
    internal class CreateOrGetExistedPostgreSqlDatabaseHostStartupAction : IHostStartupAction,
                                                                           ICollectionResolvable<IHostStartupAction>,
                                                                           IResolvable<CreateOrGetExistedPostgreSqlDatabaseHostStartupAction>
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

        private readonly SqlDatabaseSettings _sqlDatabaseSettings;
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public CreateOrGetExistedPostgreSqlDatabaseHostStartupAction(
            ISettingsProvider<SqlDatabaseSettings> sqlDatabaseSettingsProvider,
            IDependencyContainer dependencyContainer,
            IDatabaseConnectionProvider connectionProvider)
        {
            _sqlDatabaseSettings = sqlDatabaseSettingsProvider.Get();

            _dependencyContainer = dependencyContainer;
            _connectionProvider = connectionProvider;
        }

        [SuppressMessage("Analysis", "CA2000", Justification = "IDbConnection will be disposed in outer scope by client")]
        public async Task Run(CancellationToken token)
        {
            var command = new SqlCommand(
                CommandText.Format(_sqlDatabaseSettings.Database, _sqlDatabaseSettings.Username, _sqlDatabaseSettings.Password),
                Array.Empty<SqlCommandParameter>());

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = _sqlDatabaseSettings.Host,
                Port = _sqlDatabaseSettings.Port,
                Database = "postgres",
                Username = _sqlDatabaseSettings.Username,
                Password = _sqlDatabaseSettings.Password
            };

            var npgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);

            try
            {
                await npgSqlConnection.OpenAsync(token).ConfigureAwait(false);

                _ = await _connectionProvider
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
        }
    }
}