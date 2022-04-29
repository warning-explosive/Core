namespace SpaceEngineers.Core.GenericEndpoint.Host.StartupActions
{
    using System.Threading;
    using System.Threading.Tasks;
    using Basics.Attributes;
    using CompositionRoot.Api.Abstractions;
    using DataAccess.StartupActions;
    using Endpoint;
    using GenericHost.Api.Abstractions;

    [Dependency(typeof(GenericEndpointOutboxHostStartupAction))]
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