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
    using CrossCuttingConcerns.Settings;
    using Settings;
    using Transaction;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelBuilder : IDatabaseModelBuilder,
                                          IResolvable<IDatabaseModelBuilder>
    {
        private readonly ISettingsProvider<SqlDatabaseSettings> _settingsProvider;

        public DatabaseModelBuilder(ISettingsProvider<SqlDatabaseSettings> settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task<DatabaseNode?> BuildModel(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
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

            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            return new DatabaseNode(settings.Host, settings.Database, schemas);
        }

        private static async Task<SchemaNode> BuildSchemaNode(
            IDatabaseContext databaseContext,
            string schema,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, DatabaseColumnConstraint>>> constraints,
            CancellationToken token)
        {
            var types = await BuildEnumTypeNodes(databaseContext, schema, token).ConfigureAwait(false);
            var tables = await BuildTableNodes(databaseContext, schema, constraints, token).ConfigureAwait(false);
            var views = await BuildViewNodes(databaseContext, schema, token).ConfigureAwait(false);
            var indexes = await BuildIndexNodes(databaseContext, schema, token).ConfigureAwait(false);

            return new SchemaNode(schema, types, tables, views, indexes);
        }

        private static async Task<IReadOnlyCollection<EnumTypeNode>> BuildEnumTypeNodes(
            IDatabaseContext databaseContext,
            string schema,
            CancellationToken token)
        {
            return (await databaseContext
                    .All<DatabaseEnumType>()
                    .Where(column => column.Schema == schema)
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .GroupBy(column => column.Type, column => column.Value)
                .Select(grp => BuildEnumTypeNode(schema, grp.Key, grp.ToList()))
                .ToList();
        }

        private static EnumTypeNode BuildEnumTypeNode(
            string schema,
            string name,
            IReadOnlyCollection<string> values)
        {
            return new EnumTypeNode(schema, name, values);
        }

        private static async Task<IReadOnlyCollection<TableNode>> BuildTableNodes(
            IDatabaseContext databaseContext,
            string schema,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, DatabaseColumnConstraint>>> constraints,
            CancellationToken token)
        {
            return (await databaseContext
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
            IDatabaseContext databaseContext,
            string schema,
            CancellationToken token)
        {
            return (await databaseContext
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
            IDatabaseContext databaseContext,
            string schema,
            CancellationToken token)
        {
            return (await databaseContext
                    .All<DatabaseIndex>()
                    .Where(index => index.Schema == schema)
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .Select(index => IndexNode.FromDb(index.Schema, index.Table, index.Index, index.Definition))
                .ToList();
        }
    }
}