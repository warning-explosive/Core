namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Host.StartupActions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics.Attributes;
    using Basics.Primitives;
    using Contract;
    using GenericEndpoint.Host.StartupActions;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.CrossCuttingConcerns.Extensions;
    using SpaceEngineers.Core.DataAccess.Orm.Host.Abstractions;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    [Before(typeof(GenericEndpointHostStartupAction))]
    internal class UpgradeDatabaseHostStartupAction : IHostStartupAction,
                                                      ICollectionResolvable<IHostStartupAction>,
                                                      IResolvable<UpgradeDatabaseHostStartupAction>
    {
        private static readonly AsyncAutoResetEvent Sync = new AsyncAutoResetEvent(true);

        private readonly EndpointIdentity _endpointIdentity;
        private readonly IMigrationsExecutor _migrationsExecutor;
        private readonly IEnumerable<IMigration> _migrations;
        private readonly ILogger _logger;

        public UpgradeDatabaseHostStartupAction(
            EndpointIdentity endpointIdentity,
            IMigrationsExecutor migrationsExecutor,
            IEnumerable<IMigration> migrations,
            ILogger logger)
        {
            _endpointIdentity = endpointIdentity;
            _logger = logger;
            _migrationsExecutor = migrationsExecutor;
            _migrations = migrations;
        }

        public async Task Run(CancellationToken token)
        {
            try
            {
                await Sync
                   .WaitAsync(token)
                   .ConfigureAwait(false);

                await _migrationsExecutor
                   .Migrate(_migrations.ToList(), token)
                   .ConfigureAwait(false);
            }
            finally
            {
                Sync.Set();
            }

            _logger.Information($"{_endpointIdentity} have been migrated");
        }
    }
}