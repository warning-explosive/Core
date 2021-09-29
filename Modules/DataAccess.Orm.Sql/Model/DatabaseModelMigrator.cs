namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelMigrator : IDatabaseModelMigrator
    {
        public Task Migrate(IReadOnlyCollection<IDatabaseModelChange> modelChanges, CancellationToken token)
        {
            throw new System.NotImplementedException("#110 - Model builder & migrations");
        }
    }
}