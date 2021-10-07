namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System;
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
    internal class CreateTableDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateTable>
    {
        private const string CommandFormat = @"create table ""{0}"".""{1}""
(
	{2}
)";

        private const string ColumnFormat = @"""{0}"" {1}{2}";

        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IAdvancedDatabaseTransaction _databaseTransaction;
        private readonly CreateColumnDatabaseModelChangeMigration _createColumnMigration;

        public CreateTableDatabaseModelChangeMigration(
            ISettingsProvider<OrmSettings> settingsProvider,
            IAdvancedDatabaseTransaction databaseTransaction,
            CreateColumnDatabaseModelChangeMigration createColumnMigration)
        {
            _settingsProvider = settingsProvider;
            _databaseTransaction = databaseTransaction;
            _createColumnMigration = createColumnMigration;
        }

        public async Task Migrate(CreateTable change, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var columns = change
                .Columns
                .Select(CreateColumn)
                .ToString($",{Environment.NewLine}\t");

            var command = new CommandDefinition(
                CommandFormat.Format(change.Schema, change.Table, columns),
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

        private string CreateColumn(CreateColumn createColumn)
        {
            var (columnName, dataType, modifiers) = _createColumnMigration.CreateColumn(createColumn);

            string constraints;

            if (modifiers.Any())
            {
                var separator = " ";
                constraints = separator + modifiers.ToString(separator);
            }
            else
            {
                constraints = string.Empty;
            }

            return ColumnFormat.Format(columnName, dataType, constraints);
        }
    }
}