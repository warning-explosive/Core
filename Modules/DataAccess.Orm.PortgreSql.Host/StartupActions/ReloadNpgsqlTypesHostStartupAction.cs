namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.StartupActions
{
    using System.Threading;
    using System.Threading.Tasks;
    using Basics.Attributes;
    using Npgsql;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Connection;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;

    [Component(EnLifestyle.Singleton)]
    [Before("SpaceEngineers.Core.GenericEndpoint.Host SpaceEngineers.Core.GenericEndpoint.Host.StartupActions.GenericEndpointHostStartupAction")]
    [After("SpaceEngineers.Core.GenericEndpoint.DataAccess.Host SpaceEngineers.Core.GenericEndpoint.DataAccess.Host.StartupActions.UpgradeDatabaseHostStartupAction")]
    internal class ReloadNpgsqlTypesHostStartupAction : IHostStartupAction,
                                                        ICollectionResolvable<IHostStartupAction>,
                                                        IResolvable<ReloadNpgsqlTypesHostStartupAction>
    {
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public ReloadNpgsqlTypesHostStartupAction(IDatabaseConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public async Task Run(CancellationToken token)
        {
            var connection = await _connectionProvider
                .OpenConnection(token)
                .ConfigureAwait(false);

            using (connection)
            {
                await ((NpgsqlConnection)connection)
                    .ReloadTypesAsync()
                    .ConfigureAwait(false);
            }
        }
    }
}