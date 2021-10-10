namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Database;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateTableModelChangeMigration : IModelChangeMigration<CreateTable>
    {
        private readonly IInMemoryDatabase _database;

        public CreateTableModelChangeMigration(IInMemoryDatabase database)
        {
            _database = database;
        }

        public Task Migrate(CreateTable change, CancellationToken token)
        {
            var type = AssembliesExtensions.FindRequiredType(change.Schema, change.Table);

            return _database.CreateTable(type, token);
        }
    }
}