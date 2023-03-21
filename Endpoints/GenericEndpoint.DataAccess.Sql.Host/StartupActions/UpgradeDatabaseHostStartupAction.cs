namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host.StartupActions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Attributes;
    using CrossCuttingConcerns.Logging;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations;
    using SpaceEngineers.Core.GenericEndpoint.Host.StartupActions;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    [Before(typeof(GenericEndpointHostStartupAction))]
    internal class UpgradeDatabaseHostStartupAction : IHostStartupAction,
                                                      ICollectionResolvable<IHostStartupAction>,
                                                      IResolvable<UpgradeDatabaseHostStartupAction>
    {
        private static readonly Exclusive Exclusive = new Exclusive();

        private readonly IMigrationsExecutor _migrationsExecutor;
        private readonly IEnumerable<IMigration> _migrations;
        private readonly ILogger _logger;

        public UpgradeDatabaseHostStartupAction(
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