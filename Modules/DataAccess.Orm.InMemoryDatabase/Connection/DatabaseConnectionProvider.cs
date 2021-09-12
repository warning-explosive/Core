namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Connection
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using Orm.Connection;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseConnectionProvider : IDatabaseConnectionProvider
    {
        private readonly ISettingsProvider<InMemoryDatabaseSettings> _settingsProvider;

        public DatabaseConnectionProvider(ISettingsProvider<InMemoryDatabaseSettings> settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public string Database => Settings(CancellationToken.None).Result.Database;

        public IsolationLevel IsolationLevel => Settings(CancellationToken.None).Result.IsolationLevel;

        public Task<bool> DoesDatabaseExist(CancellationToken token)
        {
            return Task.FromResult(true);
        }

        public async Task<IDbConnection> OpenConnection(CancellationToken token)
        {
            var settings = await Settings(token).ConfigureAwait(false);

            var connection = new InMemoryDbConnection(settings.Database, settings.IsolationLevel);
            connection.Open();

            return connection;
        }

        private Task<InMemoryDatabaseSettings> Settings(CancellationToken token)
        {
            return _settingsProvider.Get(token);
        }
    }
}