namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Model;
    using Sql.Migrations;

    [Component(EnLifestyle.Singleton)]
    internal class CreateTableDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateTable>
    {
        private const string CommandFormat = @"create table ""{0}"".""{1}""
(
	{2}
)";

        private const string ColumnFormat = @"""{0}"" {1}{2}";

        private readonly CreateColumnDatabaseModelChangeMigration _createColumnMigration;

        public CreateTableDatabaseModelChangeMigration(CreateColumnDatabaseModelChangeMigration createColumnMigration)
        {
            _createColumnMigration = createColumnMigration;
        }

        public Task<string> Migrate(CreateTable change, CancellationToken token)
        {
            // TODO: #110 - create FK constraints and define on delete actions
            var columns = change
                .Columns
                .Select(CreateColumn)
                .ToString($",{Environment.NewLine}\t");

            var command = CommandFormat.Format(change.Schema, change.Table, columns);

            return Task.FromResult(command);
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