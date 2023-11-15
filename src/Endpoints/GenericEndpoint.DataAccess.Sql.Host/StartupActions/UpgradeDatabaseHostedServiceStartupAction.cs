namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host.StartupActions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Attributes;
    using Core.DataAccess.Orm.Sql.Migrations.Internals;
    using CrossCuttingConcerns.Logging;
    using GenericHost;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.GenericEndpoint.Host.StartupActions;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    [Before(typeof(GenericEndpointHostedServiceStartupAction))]
    internal class UpgradeDatabaseHostedServiceStartupAction : IHostedServiceStartupAction,
                                                               ICollectionResolvable<IHostedServiceObject>,
                                                               ICollectionResolvable<IHostedServiceStartupAction>,
                                                               IResolvable<UpgradeDatabaseHostedServiceStartupAction>
    {
        private static readonly Exclusive Exclusive = new Exclusive();

        private readonly IMigrationsExecutor _migrationsExecutor;
        private readonly IEnumerable<IMigration> _migrations;
        private readonly ILogger _logger;

        public UpgradeDatabaseHostedServiceStartupAction(
            IMigrationsExecutor migrationsExecutor,
            IEnumerable<IMigration> migrations,
            ILogger logger)
        {
            _logger = logger;
            _migrationsExecutor = migrationsExecutor;
            _migrations = migrations;
        }

        public async Task Run(CancellationToken token)
        {
            using (await Exclusive.Run(token).ConfigureAwait(false))
            {
                await _migrationsExecutor
                   .Migrate(_migrations.ToList(), token)
                   .ConfigureAwait(false);
            }

            _logger.Information("Endpoint has been migrated");
        }
    }
}