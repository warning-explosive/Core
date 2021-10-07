namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CrossCuttingConcerns.Api.Abstractions;
    using Dapper;
    using Orm.Model;
    using Orm.Settings;

    [Component(EnLifestyle.Scoped)]
    internal class CreateViewDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateView>
    {
        private const string CommandFormat = @"create view ""{0}"".""{1}"" as {2}";

        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IAdvancedDatabaseTransaction _databaseTransaction;

        public CreateViewDatabaseModelChangeMigration(
            ISettingsProvider<OrmSettings> settingsProvider,
            IAdvancedDatabaseTransaction databaseTransaction)
        {
            _settingsProvider = settingsProvider;
            _databaseTransaction = databaseTransaction;
        }

        public async Task Migrate(CreateView change, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var command = new CommandDefinition(
                CommandFormat.Format(change.Schema, change.View, change.Query),
                null,
                _databaseTransaction.UnderlyingDbTransaction,
                settings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            await _databaseTransaction
                .UnderlyingDbTransaction
                .Connection
                .ExecuteAsync(command)
                .ConfigureAwait(false);
        }
    }
}