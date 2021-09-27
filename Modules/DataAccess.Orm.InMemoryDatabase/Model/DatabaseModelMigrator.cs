namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Database;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelMigrator : IDatabaseModelMigrator
    {
        private readonly IInMemoryDatabase _database;

        public DatabaseModelMigrator(IInMemoryDatabase database)
        {
            _database = database;
        }

        public async Task Migrate(IReadOnlyCollection<IDatabaseModelChange> modelChanges, CancellationToken token)
        {
            var databaseName = modelChanges
                .OfType<CreateDatabase>()
                .Single()
                .Name;

            if (!_database.Name.Equals(databaseName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Expected to create {_database.Name} instead of {databaseName}");
            }

            var tables = modelChanges
                .OfType<CreateTable>()
                .Select(change => change.Table.Type!);

            foreach (var table in tables)
            {
                await _database
                    .CreateTable(table, token)
                    .ConfigureAwait(false);
            }
        }
    }
}