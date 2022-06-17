namespace SpaceEngineers.Core.GenericEndpoint.Host.StartupActions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Attributes;
    using CompositionRoot.Api.Abstractions;
    using Contract;
    using DataAccess.StartupActions;
    using Endpoint;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;
    using Microsoft.Extensions.Logging;

    [Dependency(typeof(GenericEndpointInboxHostStartupAction))]
    internal class GenericEndpointHostStartupAction : IHostStartupAction
    {
        private readonly IDependencyContainer _dependencyContainer;

        public GenericEndpointHostStartupAction(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public Task Run(CancellationToken token)
        {
            var logger = _dependencyContainer.Resolve<ILogger>();
            var transport = _dependencyContainer.Resolve<IIntegrationTransport>();
            var endpointIdentity = _dependencyContainer.Resolve<EndpointIdentity>();

            transport.BindErrorHandler(endpointIdentity, ErrorMessageHandler(logger, endpointIdentity));

            token.Register(
                () => _dependencyContainer.Resolve<IRunnableEndpoint>().StopAsync(token),
                useSynchronizationContext: false);

            return _dependencyContainer
                .Resolve<IRunnableEndpoint>()
                .StartAsync(token);
        }

        private static Func<IntegrationMessage, Exception, CancellationToken, Task> ErrorMessageHandler(
            ILogger logger,
            EndpointIdentity endpointIdentity)
        {
            return (_, exception, _) =>
            {
                logger.Error(exception, $"{endpointIdentity} -> Message handling error");
                return Task.CompletedTask;
            };
        }
    }
}