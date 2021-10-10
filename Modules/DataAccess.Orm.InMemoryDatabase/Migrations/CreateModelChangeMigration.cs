namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Migrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Database;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateModelChangeMigration : IModelChangeMigration<CreateDatabase>
    {
        private readonly IInMemoryDatabase _database;

        public CreateModelChangeMigration(IInMemoryDatabase database)
        {
            _database = database;
        }

        public Task Migrate(CreateDatabase change, CancellationToken token)
        {
            if (!_database.Name.Equals(change.Database, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Expected to create {_database.Name} instead of {change.Database}");
            }

            return Task.CompletedTask;
        }
    }
}