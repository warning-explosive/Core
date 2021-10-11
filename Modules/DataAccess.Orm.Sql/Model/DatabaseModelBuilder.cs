namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Reading;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Connection;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelBuilder : IDatabaseModelBuilder
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseConnectionProvider _connectionProvider;
        private readonly IModelProvider _modelProvider;

        public DatabaseModelBuilder(
            IDependencyContainer dependencyContainer,
            IDatabaseConnectionProvider connectionProvider,
            IModelProvider modelProvider)
        {
            _dependencyContainer = dependencyContainer;
            _connectionProvider = connectionProvider;
            _modelProvider = modelProvider;
        }

        public async Task<DatabaseNode?> BuildModel(CancellationToken token)
        {
            var databaseExists = await _connectionProvider
                .DoesDatabaseExist(token)
                .ConfigureAwait(false);

            if (!databaseExists)
            {
                return default;
            }

            await using (_dependencyContainer.OpenScopeAsync())
            {
                var transaction = _dependencyContainer.Resolve<IDatabaseTransaction>();

                await using (await transaction.Open(true, token).ConfigureAwait(false))
                {
                    var schemas = await (await transaction
                            .Read<DatabaseSchema, Guid>()
                            .All()
                            .Select(schema => schema.Name)
                            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, token)
                            .ConfigureAwait(false))
                        .Select(schema => BuildSchemaNode(transaction, schema, token))
                        .WhenAll()
                        .ConfigureAwait(false);

                    return new DatabaseNode(_connectionProvider.Host, _connectionProvider.Database, schemas);
                }
            }
        }

        private async Task<SchemaNode> BuildSchemaNode(
            IDatabaseContext transaction,
            string schema,
            CancellationToken token)
        {
            var tables = await BuildTableNodes(transaction, schema, token).ConfigureAwait(false);
            var views = await BuildViewNodes(transaction, schema, token).ConfigureAwait(false);
            var indexes = await BuildIndexNodes(transaction, schema, token).ConfigureAwait(false);

            return new SchemaNode(schema, tables, views, indexes);
        }

        private async Task<IReadOnlyCollection<TableNode>> BuildTableNodes(
            IDatabaseContext transaction,
            string schema,
            CancellationToken token)
        {
            return (await transaction
                    .Read<DatabaseColumn, Guid>()
                    .All()
                    .Where(column => column.Schema == schema)
                    .GroupBy(column => column.Table)
                    .ToDictionaryAsync(grp => grp.Key, grp => grp.ToList(), token)
                    .ConfigureAwait(false))
                .Select(grp => BuildTableNode(schema, grp.Key, grp.Value))
                .ToList();
        }

        private TableNode BuildTableNode(string schema, string table, IReadOnlyCollection<DatabaseColumn> databaseColumns)
        {
            var columns = databaseColumns
                .Select(column => BuildColumnNode(schema, table, column))
                .ToList();

            return new TableNode(schema, table, columns);
        }

        private ColumnNode BuildColumnNode(string schema, string table, DatabaseColumn column)
        {
            var constraints = ColumnInfo
                .DbConstraints(schema, table, column.Column, column.Nullable, _modelProvider)
                .ToList();

            return new ColumnNode(column.Schema, column.Table, column.Column, column.DataType, constraints);
        }

        private static async Task<List<ViewNode>> BuildViewNodes(
            IDatabaseContext transaction,
            string schema,
            CancellationToken token)
        {
            return (await transaction
                    .Read<DatabaseView, Guid>()
                    .All()
                    .Where(view => view.Schema == schema)
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .Select(BuildViewNode(schema))
                .ToList();

            static Func<DatabaseView, ViewNode> BuildViewNode(string schema)
            {
                return view => new ViewNode(schema, view.View, view.Query);
            }
        }

        private static async Task<IReadOnlyCollection<IndexNode>> BuildIndexNodes(
            IDatabaseContext transaction,
            string schema,
            CancellationToken token)
        {
            return (await transaction
                    .Read<DatabaseIndex, Guid>()
                    .All()
                    .Where(index => index.Schema == schema)
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .Select(index => IndexNode.FromDb(index.Schema, index.Table, index.Index))
                .ToList();
        }
    }
}