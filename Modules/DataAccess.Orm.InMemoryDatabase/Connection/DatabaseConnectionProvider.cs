namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Connection
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using Database;
    using Orm.Connection;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseConnectionProvider : IDatabaseConnectionProvider
    {
        private readonly ISettingsProvider<InMemoryDatabaseSettings> _settingsProvider;
        private readonly InMemoryDatabase _inMemoryDatabase;

        public DatabaseConnectionProvider(
            ISettingsProvider<InMemoryDatabaseSettings> settingsProvider,
            InMemoryDatabase inMemoryDatabase)
        {
            _settingsProvider = settingsProvider;
            _inMemoryDatabase = inMemoryDatabase;
        }

        public string Host => nameof(InMemoryDatabase);

        public string Database => _settingsProvider.Get(CancellationToken.None).Result.Database;

        public IsolationLevel IsolationLevel => _settingsProvider.Get(CancellationToken.None).Result.IsolationLevel;

        public Task<bool> DoesDatabaseExist(CancellationToken token)
        {
            return Task.FromResult(true);
        }

        public async Task<IDbConnection> OpenConnection(CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var connection = new InMemoryDbConnection(settings.Database, settings.IsolationLevel, _inMemoryDatabase);
            connection.Open();

            return connection;
        }
    }
}