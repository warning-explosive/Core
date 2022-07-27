namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.Api.Exceptions;
    using CrossCuttingConcerns.Settings;
    using Extensions;
    using Npgsql;
    using Orm.Connection;
    using Sql.Settings;
    using Transaction;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseConnectionProvider : IDatabaseConnectionProvider,
                                                IResolvable<IDatabaseConnectionProvider>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<SqlDatabaseSettings> _settingsProvider;

        public DatabaseConnectionProvider(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<SqlDatabaseSettings> settingsProvider)
        {
            _dependencyContainer = dependencyContainer;
            _settingsProvider = settingsProvider;
        }

        public string Host => _settingsProvider.Get(CancellationToken.None).Result.Host;

        public string Database => _settingsProvider.Get(CancellationToken.None).Result.Database;

        public IsolationLevel IsolationLevel => _settingsProvider.Get(CancellationToken.None).Result.IsolationLevel;

        public Task<bool> DoesDatabaseExist(CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(DoesDatabaseExistUnsafe)
                .Catch<PostgresException>()
                .Invoke(static (exception, _) => Task.FromResult(!exception.DatabaseDoesNotExist()), token);
        }

        [SuppressMessage("Analysis", "CA2000", Justification = "IDbConnection will be disposed in outer scope by client")]
        public async Task<IDatabaseConnection> OpenConnection(CancellationToken token)
        {
            ValidateNestedCall(_dependencyContainer);

            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = settings.Host,
                Port = settings.Port,
                Database = settings.Database,
                Username = settings.Username,
                Password = settings.Password,
                Pooling = true,
                MinPoolSize = 0,
                MaxPoolSize = (int)settings.ConnectionPoolSize,
                ConnectionPruningInterval = 1,
                ConnectionIdleLifetime = 1,
                IncludeErrorDetail = true
            };

            var npgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);

            var connection = new DatabaseConnection(npgSqlConnection);

            await npgSqlConnection
               .OpenAsync(token)
               .ConfigureAwait(false);

            return connection;
        }

        private static void ValidateNestedCall(IDependencyContainer dependencyContainer)
        {
            var transaction = ExecutionExtensions
               .Try(dependencyContainer, container => (IAdvancedDatabaseTransaction?)container.Resolve<IAdvancedDatabaseTransaction>())
               .Catch<ComponentResolutionException>()
               .Invoke(_ => default);

            if (transaction?.Connected == true)
            {
                throw new InvalidOperationException("Nested database connections aren't supported");
            }
        }

        private async Task<bool> DoesDatabaseExistUnsafe(CancellationToken token)
        {
            using (await OpenConnection(token).ConfigureAwait(false))
            {
                return true;
            }
        }
    }
}