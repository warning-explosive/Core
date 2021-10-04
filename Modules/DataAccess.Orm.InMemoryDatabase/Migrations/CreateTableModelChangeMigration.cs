namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Database;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateTableModelChangeMigration : IDatabaseModelChangeMigration<CreateTable>
    {
        private readonly IInMemoryDatabase _database;

        public CreateTableModelChangeMigration(IInMemoryDatabase database)
        {
            _database = database;
        }

        public Task Migrate(CreateTable change, CancellationToken token)
        {
            return _database.CreateTable(change.Type, token);
        }
    }
}