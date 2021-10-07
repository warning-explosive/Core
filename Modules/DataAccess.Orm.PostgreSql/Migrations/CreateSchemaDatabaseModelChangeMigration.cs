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
    internal class CreateSchemaDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateSchema>
    {
        private const string CommandFormat = @"create schema ""{0}""";

        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IAdvancedDatabaseTransaction _databaseTransaction;

        public CreateSchemaDatabaseModelChangeMigration(
            ISettingsProvider<OrmSettings> settingsProvider,
            IAdvancedDatabaseTransaction databaseTransaction)
        {
            _settingsProvider = settingsProvider;
            _databaseTransaction = databaseTransaction;
        }

        public async Task Migrate(CreateSchema change, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var command = new CommandDefinition(
                CommandFormat.Format(change.Schema),
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