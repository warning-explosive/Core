namespace SpaceEngineers.Core.GenericEndpoint.Host.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using CompositionRoot.Api.Abstractions;
    using GenericEndpoint.Abstractions;
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
            return _dependencyContainer
                .Resolve<IRunnableEndpoint>()
                .StartAsync(token);
        }
    }
}