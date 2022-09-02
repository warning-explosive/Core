namespace SpaceEngineers.Core.GenericEndpoint.Host.StartupActions
{
    using System.Threading;
    using System.Threading.Tasks;
    using CompositionRoot;
    using Endpoint;
    using GenericHost.Api.Abstractions;

    internal class GenericEndpointHostStartupAction : IHostStartupAction
    {
        private readonly IDependencyContainer _dependencyContainer;

        public GenericEndpointHostStartupAction(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public Task Run(CancellationToken token)
        {
            token.Register(
                () => _dependencyContainer.Resolve<IExecutableEndpoint>().StopAsync(token),
                useSynchronizationContext: false);

            return _dependencyContainer
                .Resolve<IExecutableEndpoint>()
                .StartAsync(token);
        }
    }
}