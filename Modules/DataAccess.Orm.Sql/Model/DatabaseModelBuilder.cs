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
    using Reading;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelBuilder : IDatabaseModelBuilder
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public DatabaseModelBuilder(
            IDependencyContainer dependencyContainer,
            IDatabaseConnectionProvider connectionProvider)
        {
            _dependencyContainer = dependencyContainer;
            _connectionProvider = connectionProvider;
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
                    var defaultSchemas = new[]
                    {
                        "information_schema",
                        "public"
                    };

                    var schemas = await (await transaction
                            .Read<DatabaseSchema, Guid>()
                            .All()
                            .Select(schema => schema.Name)
                            .Where(schema => !defaultSchemas.Contains(schema) && !schema.Like("pg_%"))
                            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, token)
                            .ConfigureAwait(false))
                        .Select(schema => BuildSchemaNode(transaction, schema, token))
                        .WhenAll()
                        .ConfigureAwait(false);

                    return new DatabaseNode(_connectionProvider.Host, _connectionProvider.Database, schemas);
                }
            }
        }

        private static async Task<SchemaNode> BuildSchemaNode(
            IDatabaseContext transaction,
            string schema,
            CancellationToken token)
        {
            var tables = await BuildTableNodes(transaction, schema, token).ConfigureAwait(false);
            var views = await BuildViewNodes(transaction, schema, token).ConfigureAwait(false);
            var indexes = await BuildIndexNodes(transaction, schema, token).ConfigureAwait(false);

            return new SchemaNode(schema, tables, views, indexes);
        }

        private static async Task<IReadOnlyCollection<TableNode>> BuildTableNodes(
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

            static TableNode BuildTableNode(string schema, string table, IReadOnlyCollection<DatabaseColumn> databaseColumns)
            {
                var columns = databaseColumns
                    .Select(BuildColumnNode)
                    .ToList();

                return new TableNode(schema, table, GetTableType(schema, table), columns);

                static Type GetTableType(string schema, string table)
                {
                    throw new NotImplementedException($"#110 - {schema}.{table}");
                }
            }

            static ColumnNode BuildColumnNode(DatabaseColumn column)
            {
                return new ColumnNode(column.Schema, column.Table, column.Column, GetColumnType(column));

                static Type GetColumnType(DatabaseColumn column)
                {
                    throw new NotImplementedException($"#110 - {column.DataType}");
                }
            }
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
                    .Select(index => index.Index)
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .Select(IndexNode.FromName)
                .ToList();
        }
    }
}