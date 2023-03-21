namespace SpaceEngineers.Core.GenericEndpoint.Host.StartupActions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Endpoint;
    using GenericHost.Api.Abstractions;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    internal class GenericEndpointHostStartupAction : IHostStartupAction,
                                                      ICollectionResolvable<IHostStartupAction>,
                                                      IResolvable<GenericEndpointHostStartupAction>
    {
        private readonly IExecutableEndpoint _executableEndpoint;

        public GenericEndpointHostStartupAction(IExecutableEndpoint executableEndpoint)
        {
            _executableEndpoint = executableEndpoint;
        }

        public Task Run(CancellationToken token)
        {
            token.Register(() => _executableEndpoint.StopAsync(token), useSynchronizationContext: false);

            return _executableEndpoint.StartAsync(token);
        }
    }
}