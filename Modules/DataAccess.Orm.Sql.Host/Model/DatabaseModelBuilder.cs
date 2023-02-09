namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Reading;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using Connection;
    using Orm.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelBuilder : IDatabaseModelBuilder,
                                          IResolvable<IDatabaseModelBuilder>
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

            return await _dependencyContainer
                .InvokeWithinTransaction(false, BuildModel, token)
                .ConfigureAwait(false);
        }

        private async Task<DatabaseNode> BuildModel(IDatabaseTransaction transaction, CancellationToken token)
        {
            var constraints = (await transaction
                    .All<DatabaseColumnConstraint>()
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .GroupBy(constraint => constraint.Schema)
                .ToDictionary(grp => grp.Key,
                    grp => grp
                        .GroupBy(constraint => constraint.Table)
                        .ToDictionary(g => g.Key,
                            g => g.ToDictionary(
                                constraint => constraint.Column,
                                StringComparer.OrdinalIgnoreCase) as IReadOnlyDictionary<string, DatabaseColumnConstraint>,
                            StringComparer.OrdinalIgnoreCase) as IReadOnlyDictionary<string, IReadOnlyDictionary<string, DatabaseColumnConstraint>>,
                    StringComparer.OrdinalIgnoreCase);

            var schemas = await (await transaction
                    .All<DatabaseSchema>()
                    .Select(schema => schema.Name)
                    .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, token)
                    .ConfigureAwait(false))
                .Select(schema => BuildSchemaNode(transaction, schema, constraints, token))
                .WhenAll()
                .ConfigureAwait(false);

            return new DatabaseNode(_connectionProvider.Host, _connectionProvider.Database, schemas);
        }

        private static async Task<SchemaNode> BuildSchemaNode(IDatabaseContext transaction,
            string schema,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, DatabaseColumnConstraint>>> constraints,
            CancellationToken token)
        {
            var tables = await BuildTableNodes(transaction, schema, constraints, token).ConfigureAwait(false);
            var views = await BuildViewNodes(transaction, schema, token).ConfigureAwait(false);
            var indexes = await BuildIndexNodes(transaction, schema, token).ConfigureAwait(false);

            return new SchemaNode(schema, tables, views, indexes);
        }

        private static async Task<IReadOnlyCollection<TableNode>> BuildTableNodes(
            IDatabaseContext transaction,
            string schema,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, DatabaseColumnConstraint>>> constraints,
            CancellationToken token)
        {
            return (await transaction
                    .All<DatabaseColumn>()
                    .Where(column => column.Schema == schema)
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .GroupBy(column => column.Table)
                .ToDictionary(grp => grp.Key, grp => grp.ToList())
                .Select(grp => BuildTableNode(schema, grp.Key, grp.Value, constraints))
                .ToList();
        }

        private static TableNode BuildTableNode(
            string schema,
            string table,
            IReadOnlyCollection<DatabaseColumn> databaseColumns,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, DatabaseColumnConstraint>>> constraints)
        {
            var columns = databaseColumns
                .Select(column => BuildColumnNode(schema, table, column, constraints))
                .ToList();

            return new TableNode(schema, table, columns);
        }

        private static ColumnNode BuildColumnNode(
            string schema,
            string table,
            DatabaseColumn column,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, DatabaseColumnConstraint>>> constraints)
        {
            return new ColumnNode(
                column.Schema,
                column.Table,
                column.Column,
                column.DataType,
                GetConstraints(schema, table, column.Column, column.Nullable, constraints)
                    .OrderBy(constraint => constraint)
                    .ToList());
        }

        private static IEnumerable<string> GetConstraints(
            string schema,
            string table,
            string column,
            bool nullable,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, DatabaseColumnConstraint>>> constraints)
        {
            if (constraints.TryGetValue(schema, out var tables)
                && tables.TryGetValue(table, out var columns)
                && columns.TryGetValue(column, out var constraint))
            {
                yield return constraint.ConstraintType switch
                {
                    EnColumnConstraintType.PrimaryKey => "primary key",
                    EnColumnConstraintType.ForeignKey => $@"references ""{constraint.ForeignSchema}"".""{constraint.ForeignTable}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")",
                    _ => throw new NotSupportedException(constraint.ConstraintType.ToString())
                };
            }

            if (!nullable)
            {
                yield return "not null";
            }
        }

        private static async Task<List<ViewNode>> BuildViewNodes(
            IDatabaseContext transaction,
            string schema,
            CancellationToken token)
        {
            return (await transaction
                    .All<DatabaseView>()
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
                    .All<DatabaseIndex>()
                    .Where(index => index.Schema == schema)
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .Select(index => IndexNode.FromDb(index.Schema, index.Table, index.Index, index.Definition))
                .ToList();
        }
    }
}