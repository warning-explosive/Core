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

                    return new DatabaseNode(_connectionProvider.Database, schemas);
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

            return new SchemaNode(schema, tables, views);
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
                    .GroupBy(column => column.TableName)
                    .ToDictionaryAsync(grp => grp.Key, grp => grp.ToList(), token)
                    .ConfigureAwait(false))
                .Select(grp => BuildTableNode(grp.Key, grp.Value))
                .ToList();

            static TableNode BuildTableNode(string tableName, IReadOnlyCollection<DatabaseColumn> databaseColumns)
            {
                var columns = databaseColumns
                    .Select(BuildColumnNode)
                    .ToList();

                return new TableNode(tableName, columns);
            }

            static ColumnNode BuildColumnNode(DatabaseColumn column)
            {
                var columnType = GetColumnType(column);

                return new ColumnNode(columnType, column.ColumnName);

                static Type GetColumnType(DatabaseColumn column)
                {
                    throw new NotImplementedException($"#110 - Model builder & migrations - {column}");
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
                .Select(BuildViewNode)
                .ToList();

            static ViewNode BuildViewNode(DatabaseView view)
            {
                return new ViewNode(view.Name, view.Query);
            }
        }
    }
}