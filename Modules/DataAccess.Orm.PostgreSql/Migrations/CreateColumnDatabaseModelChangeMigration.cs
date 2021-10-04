namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateColumnDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateColumn>
    {
        public Task Migrate(CreateColumn change, CancellationToken token)
        {
            throw new System.NotImplementedException("#110");
        }
    }
}