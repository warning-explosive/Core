namespace SpaceEngineers.Core.DataAccess.Orm.Host.Migrations
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CompositionRoot.Api.Abstractions;
    using GenericHost.Api.Abstractions;

    internal class UpgradeDatabaseHostStartupAction : IHostStartupAction
    {
        private readonly IDependencyContainer _dependencyContainer;

        public UpgradeDatabaseHostStartupAction(
            IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public Task Run(CancellationToken token)
        {
            var manualMigrations = _dependencyContainer
               .ResolveCollection<IManualMigration>()
               .ToList();

            return _dependencyContainer
               .Resolve<IModelMigrator>()
               .Upgrade(manualMigrations, token);
        }
    }
}