namespace SpaceEngineers.Core.DataAccess.Orm.Host.Migrations
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Attributes;
    using CompositionRoot.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericHost.Api.Abstractions;
    using Microsoft.Extensions.Logging;

    [Dependent("SpaceEngineers.Core.GenericEndpoint.Host.StartupActions.GenericEndpointHostStartupAction")]
    internal class UpgradeDatabaseHostStartupAction : IHostStartupAction
    {
        private readonly IDependencyContainer _dependencyContainer;

        public UpgradeDatabaseHostStartupAction(
            IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task Run(CancellationToken token)
        {
            var manualMigrations = _dependencyContainer
               .ResolveCollection<IManualMigration>()
               .ToList();

            await _dependencyContainer
               .Resolve<IModelMigrator>()
               .Upgrade(manualMigrations, token)
               .ConfigureAwait(false);

            _dependencyContainer
               .Resolve<ILogger>()
               .Information($"{_dependencyContainer.Resolve<EndpointIdentity>()} have been applied");
        }
    }
}