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
    using Sql.Model;

    [Component(EnLifestyle.Scoped)]
    internal class CreateColumnDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateColumn>
    {
        private const string CommandFormat = @"alter table ""{0}"".""{1}"" add ""{2}"" {3}{4}";

        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IAdvancedDatabaseTransaction _databaseTransaction;
        private readonly IColumnDataTypeProvider _columnDataTypeProvider;

        public CreateColumnDatabaseModelChangeMigration(
            ISettingsProvider<OrmSettings> settingsProvider,
            IAdvancedDatabaseTransaction databaseTransaction,
            IColumnDataTypeProvider columnDataTypeProvider)
        {
            _settingsProvider = settingsProvider;
            _databaseTransaction = databaseTransaction;
            _columnDataTypeProvider = columnDataTypeProvider;
        }

        public async Task Migrate(CreateColumn change, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var (columnName, dataType, modifiers) = CreateColumn(change);

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

            var command = new CommandDefinition(
                CommandFormat.Format(change.Schema, change.Table, columnName, dataType, constraints),
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

        internal (string Column, string DataType, string[] Modifiers) CreateColumn(CreateColumn createColumn)
        {
            return (createColumn.Column, _columnDataTypeProvider.GetColumnDataType(createColumn.Type), _columnDataTypeProvider.GetModifiers(createColumn.Type).ToArray());
        }
    }
}