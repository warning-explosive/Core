namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateTableDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateTable>
    {
        public Task Migrate(CreateTable change, CancellationToken token)
        {
            throw new System.NotImplementedException("#110");
        }
    }
}