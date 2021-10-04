namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateIndexDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateIndex>
    {
        public Task Migrate(CreateIndex change, CancellationToken token)
        {
            throw new NotImplementedException("#110");
        }
    }
}