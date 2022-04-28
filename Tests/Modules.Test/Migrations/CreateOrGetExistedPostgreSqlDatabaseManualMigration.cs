namespace SpaceEngineers.Core.Modules.Test.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Host.Migrations;
    using DataAccess.Orm.Settings;
    using DataAccess.Orm.Sql.Extensions;
    using DataAccess.Orm.Sql.Host.Migrations;
    using DataAccess.Orm.Sql.Settings;
    using Npgsql;

    [Dependent(typeof(InitialMigration))]
    [Component(EnLifestyle.Singleton)]
    internal class CreateOrGetExistedPostgreSqlDatabaseManualMigration : IManualMigration,
                                                                         ICollectionResolvable<IManualMigration>
    {
        private const string CommandText = @"
create extension if not exists dblink;

do
$do$
    begin
        if exists
            (select * from pg_catalog.pg_database where datname = '{0}')
        then
            raise notice 'database already exists';
        else
            PERFORM dblink_connect('host=localhost user=' || '{1}' || ' password=' || '{2}' || ' dbname=' || current_database());
            perform dblink_exec('create database ""{0}""');
            perform dblink_exec('grant all privileges on database ""{0}"" to ""{1}""');
        end if;
    end
$do$;";

        private readonly ISettingsProvider<SqlDatabaseSettings> _sqlDatabaseSettingsProvider;
        private readonly ISettingsProvider<OrmSettings> _ormSettingsProvider;

        public CreateOrGetExistedPostgreSqlDatabaseManualMigration(
            ISettingsProvider<SqlDatabaseSettings> sqlDatabaseSettingsProvider,
            ISettingsProvider<OrmSettings> ormSettingsProvider)
        {
            _sqlDatabaseSettingsProvider = sqlDatabaseSettingsProvider;
            _ormSettingsProvider = ormSettingsProvider;
        }

        public string Name { get; } = ModelMigrationsExecutor.GetAutomaticMigrationName(-1);

        public async Task ExecuteManualMigration(CancellationToken token)
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

            using (var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString))
            {
                await connection.OpenAsync(token).ConfigureAwait(false);

                _ = await connection
                   .InvokeScalar(commandText, ormSettings, token)
                   .ConfigureAwait(false);
            }

            NpgsqlConnection.ClearAllPools();
        }
    }
}