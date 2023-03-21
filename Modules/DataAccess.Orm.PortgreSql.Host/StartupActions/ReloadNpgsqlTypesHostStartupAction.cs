namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.StartupActions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using GenericHost.Api.Abstractions;
    using Npgsql;
    using Sql.Connection;

    [Component(EnLifestyle.Singleton)]
    [Before("SpaceEngineers.Core.GenericEndpoint.Host SpaceEngineers.Core.GenericEndpoint.Host.StartupActions.GenericEndpointHostStartupAction")]
    [After("SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host.StartupActions.UpgradeDatabaseHostStartupAction")]
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