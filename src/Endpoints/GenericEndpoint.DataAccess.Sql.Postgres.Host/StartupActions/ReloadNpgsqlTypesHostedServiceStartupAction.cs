namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Postgres.Host.StartupActions;

using System.Threading;
using System.Threading.Tasks;
using Basics.Attributes;
using GenericEndpoint.Host.StartupActions;
using GenericHost;
using Npgsql;
using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
using SpaceEngineers.Core.DataAccess.Orm.Sql.Connection;
using Sql.Host.StartupActions;

[ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
[Before(typeof(GenericEndpointHostedServiceStartupAction))]
[After(typeof(UpgradeDatabaseHostedServiceStartupAction))]
internal class ReloadNpgsqlTypesHostedServiceStartupAction : IHostedServiceStartupAction,
                                                             ICollectionResolvable<IHostedServiceObject>,
                                                             ICollectionResolvable<IHostedServiceStartupAction>,
                                                             IResolvable<ReloadNpgsqlTypesHostedServiceStartupAction>
{
    private readonly IDatabaseConnectionProvider _connectionProvider;

    public ReloadNpgsqlTypesHostedServiceStartupAction(IDatabaseConnectionProvider connectionProvider)
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