namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Sql.Host.Model;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateTableDatabaseModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateTable>,
                                                                  IResolvable<IModelChangeCommandBuilder<CreateTable>>
    {
        private const string CommandFormat = @"create table ""{0}"".""{1}""
(
	{2}
)";

        private const string ColumnFormat = @"""{0}"" {1}{2}";

        private readonly IModelProvider _modelProvider;
        private readonly CreateColumnModelChangeCommandBuilder _createColumnCommandBuilder;

        public CreateTableDatabaseModelChangeCommandBuilder(
            IModelProvider modelProvider,
            CreateColumnModelChangeCommandBuilder createColumnCommandBuilder)
        {
            _modelProvider = modelProvider;
            _createColumnCommandBuilder = createColumnCommandBuilder;
        }

        public Task<string> BuildCommand(CreateTable change, CancellationToken token)
        {
            if (!_modelProvider.TablesMap.TryGetValue(change.Schema, out var schema)
                || !schema.TryGetValue(change.Table, out var info)
                || info is not TableInfo table)
            {
                throw new InvalidOperationException($"{change.Schema}.{change.Table} isn't presented in the model");
            }

            var columns = table
                .Columns
                .Values
                .Where(column => !column.IsMultipleRelation)
                .Select(CreateColumn)
                .ToString($",{Environment.NewLine}\t");

            var commandText = CommandFormat.Format(change.Schema, change.Table, columns);

            return Task.FromResult(commandText);
        }

        private string CreateColumn(ColumnInfo column)
        {
            var (columnName, dataType, constraints) = _createColumnCommandBuilder.CreateColumn(column);

            return ColumnFormat.Format(
                columnName,
                dataType,
                constraints.Any() ? " " + constraints.ToString(" ") : string.Empty);
        }
    }
}