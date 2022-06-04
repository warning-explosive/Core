namespace SpaceEngineers.Core.GenericEndpoint.UnitOfWork
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class OutboxDelivery : IOutboxDelivery,
                                    IResolvable<IOutboxDelivery>
    {
        private readonly IIntegrationTransport _transport;

        public OutboxDelivery(
            IIntegrationTransport transport)
        {
            _transport = transport;
        }

        public async Task DeliverMessages(
            IReadOnlyCollection<IntegrationMessage> messages,
            CancellationToken token)
        {
            foreach (var message in messages)
            {
                _ = await _transport
                   .Enqueue(message, token)
                   .ConfigureAwait(false);
            }
        }
    }
}