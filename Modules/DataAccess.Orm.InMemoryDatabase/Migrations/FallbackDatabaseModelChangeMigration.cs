namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class FallbackDatabaseModelChangeMigration<TChange> : IDatabaseModelChangeMigration<TChange>
        where TChange : IDatabaseModelChange
    {
        public Task Migrate(TChange change, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}