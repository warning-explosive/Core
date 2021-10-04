namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Model
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Database;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateDatabase>
    {
        private readonly IInMemoryDatabase _database;

        public CreateDatabaseModelChangeMigration(IInMemoryDatabase database)
        {
            _database = database;
        }

        public Task Migrate(CreateDatabase change, CancellationToken token)
        {
            if (!_database.Name.Equals(change.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Expected to create {_database.Name} instead of {change.Name}");
            }

            return Task.CompletedTask;
        }
    }
}