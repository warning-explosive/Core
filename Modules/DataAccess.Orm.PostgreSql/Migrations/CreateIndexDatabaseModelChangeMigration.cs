namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System.Data;
    using System.Linq;
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
    internal class CreateIndexDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateIndex>
    {
        private const string UniqieCommandFormat = @"create unique index ""{2}"" on ""{0}"".""{1}"" ({3})";
        private const string CommandFormat = @"create index ""{2}"" on ""{0}"".""{1}"" ({3})";

        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IAdvancedDatabaseTransaction _databaseTransaction;

        public CreateIndexDatabaseModelChangeMigration(
            ISettingsProvider<OrmSettings> settingsProvider,
            IAdvancedDatabaseTransaction databaseTransaction)
        {
            _settingsProvider = settingsProvider;
            _databaseTransaction = databaseTransaction;
        }

        public async Task Migrate(CreateIndex change, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var columns = change
                .Columns
                .Select(column => $@"""{column}""")
                .ToString(", ");

            var commandTextFormat = change.Unique
                ? UniqieCommandFormat
                : CommandFormat;

            var command = new CommandDefinition(
                commandTextFormat.Format(change.Schema, change.Table, change.Index, columns),
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