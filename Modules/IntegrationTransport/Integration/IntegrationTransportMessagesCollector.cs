namespace SpaceEngineers.Core.IntegrationTransport.Integration
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Pipeline;

    [ComponentOverride]
    internal class IntegrationTransportMessagesCollector : IMessagesCollector,
                                                           IResolvable<IMessagesCollector>
    {
        private readonly IIntegrationTransport _transport;

        public IntegrationTransportMessagesCollector(IIntegrationTransport transport)
        {
            _transport = transport;
        }

        public Task Collect(IntegrationMessage message, CancellationToken token)
        {
            return _transport.Enqueue(message, token);
        }
    }
}