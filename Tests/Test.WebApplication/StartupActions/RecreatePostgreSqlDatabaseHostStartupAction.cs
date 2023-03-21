namespace SpaceEngineers.Core.Test.WebApplication.StartupActions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Sql.Connection;
    using DataAccess.Orm.Sql.Settings;
    using DataAccess.Orm.Sql.Translation;
    using GenericHost.Api.Abstractions;
    using Npgsql;

    [Component(EnLifestyle.Singleton)]
    internal class RecreatePostgreSqlDatabaseHostStartupAction : IHostStartupAction,
                                                                 ICollectionResolvable<IHostStartupAction>,
                                                                 IResolvable<RecreatePostgreSqlDatabaseHostStartupAction>
    {
        private const string CommandText = @"create extension if not exists dblink;

drop database if exists ""{0}"" with (FORCE);
create database ""{0}"";
grant all privileges on database ""{0}"" to ""{1}"";";

        private readonly SqlDatabaseSettings _sqlDatabaseSettings;
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public RecreatePostgreSqlDatabaseHostStartupAction(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<SqlDatabaseSettings> sqlDatabaseSettingsProvider,
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
                CommandText.Format(_sqlDatabaseSettings.Database, _sqlDatabaseSettings.Username),
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
                    .Execute(npgSqlConnection, command, token)
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