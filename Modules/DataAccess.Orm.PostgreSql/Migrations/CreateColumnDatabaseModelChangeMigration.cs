namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Model;
    using Sql.Migrations;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateColumnDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateColumn>
    {
        private const string CommandFormat = @"alter table ""{0}"".""{1}"" add ""{2}"" {3}{4}";

        private readonly IColumnDataTypeProvider _columnDataTypeProvider;

        public CreateColumnDatabaseModelChangeMigration(IColumnDataTypeProvider columnDataTypeProvider)
        {
            _columnDataTypeProvider = columnDataTypeProvider;
        }

        public Task<string> Migrate(CreateColumn change, CancellationToken token)
        {
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

            var command = CommandFormat.Format(change.Schema, change.Table, columnName, dataType, constraints);

            return Task.FromResult(command);
        }

        internal (string Column, string DataType, string[] Modifiers) CreateColumn(CreateColumn createColumn)
        {
            var column = createColumn.Column;
            var dataType = _columnDataTypeProvider.GetColumnDataType(createColumn.Type);
            var modifiers = _columnDataTypeProvider.GetModifiers(createColumn).ToArray();

            return (column, dataType, modifiers);
        }
    }
}