namespace SpaceEngineers.Core.IntegrationTransport.InMemory
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;

    /// <summary>
    /// IConfigurableIntegrationTransport
    /// </summary>
    internal partial class InMemoryIntegrationTransport
    {
        public void BindMessageHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Task> messageHandler,
            IIntegrationTypeProvider integrationTypeProvider)
        {
            _topology.BindMessageHandler(endpointIdentity, messageHandler, integrationTypeProvider.EndpointCommands());
            _topology.BindMessageHandler(endpointIdentity, messageHandler, integrationTypeProvider.EventsSubscriptions());
            _topology.BindMessageHandler(endpointIdentity, messageHandler, integrationTypeProvider.EndpointRequests());
            _topology.BindMessageHandler(endpointIdentity, messageHandler, integrationTypeProvider.RepliesSubscriptions());
        }

        public void BindErrorHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Exception, CancellationToken, Task> errorMessageHandler)
        {
            _topology.BindErrorHandler(endpointIdentity, errorMessageHandler);
        }

        public void LockTopologyConfiguration(EndpointIdentity endpointIdentity)
        {
            _topology.Lock(endpointIdentity);
        }
    }
}