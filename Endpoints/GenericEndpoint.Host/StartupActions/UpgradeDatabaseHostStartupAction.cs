namespace SpaceEngineers.Core.GenericEndpoint.Host.StartupActions
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics.Attributes;
    using Basics.Primitives;
    using CompositionRoot;
    using Contract;
    using Core.DataAccess.Orm.Host.Migrations;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.CrossCuttingConcerns.Extensions;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;

    [Before(typeof(GenericEndpointHostStartupAction))]
    internal class UpgradeDatabaseHostStartupAction : IHostStartupAction
    {
        private static readonly AsyncAutoResetEvent Sync = new AsyncAutoResetEvent(true);

        private readonly IDependencyContainer _dependencyContainer;

        public UpgradeDatabaseHostStartupAction(
            IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task Run(CancellationToken token)
        {
            var migrations = _dependencyContainer
               .ResolveCollection<IMigration>()
               .ToList();

            try
            {
                await Sync
                   .WaitAsync(token)
                   .ConfigureAwait(false);

                await _dependencyContainer
                   .Resolve<IMigrationsExecutor>()
                   .Migrate(migrations, token)
                   .ConfigureAwait(false);
            }
            finally
            {
                Sync.Set();
            }

            _dependencyContainer
               .Resolve<ILogger>()
               .Information($"{_dependencyContainer.Resolve<EndpointIdentity>()} have been migrated");
        }
    }
}