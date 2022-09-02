namespace SpaceEngineers.Core.IntegrationTransport.Integration
{
    using System.Threading;
    using System.Threading.Tasks;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.GenericEndpoint.Messaging;
    using SpaceEngineers.Core.GenericEndpoint.Pipeline;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;

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