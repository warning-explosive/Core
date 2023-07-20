namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Internals
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal interface IMigrationsExecutor
    {
        public Task Migrate(
            IReadOnlyCollection<IMigration> migrations,
            CancellationToken token);
    }
}