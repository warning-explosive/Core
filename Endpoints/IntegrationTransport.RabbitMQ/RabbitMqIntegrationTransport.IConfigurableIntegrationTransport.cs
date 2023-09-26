namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;

    /// <summary>
    /// IConfigurableIntegrationTransport
    /// </summary>
    internal partial class RabbitMqIntegrationTransport
    {
        public void BindMessageHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Task> messageHandler,
            IIntegrationTypeProvider integrationTypeProvider)
        {
            _endpoints.TryAdd(endpointIdentity, default);

            _integrationMessageTypes.TryAdd(endpointIdentity, integrationTypeProvider);
            _messageHandlers.TryAdd(endpointIdentity, messageHandler);

            ConfigureErrorHandler(_channels, endpointIdentity, BindErrorHandler);
        }

        public void BindErrorHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Exception, CancellationToken, Task> errorMessageHandler)
        {
            _endpoints.TryAdd(endpointIdentity, default);

            var endpointErrorHandlers = _errorMessageHandlers.GetOrAdd(endpointIdentity, new ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>());
            endpointErrorHandlers.Add(errorMessageHandler);
        }

        public void LockTopologyConfiguration(EndpointIdentity endpointIdentity)
        {
            RestartBackgroundMessageProcessing(Token).Wait(Token);
        }
    }
}