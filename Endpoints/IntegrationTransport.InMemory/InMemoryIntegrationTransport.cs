namespace SpaceEngineers.Core.IntegrationTransport.InMemory
{
    using System;
    using Api.Abstractions;
    using Api.Enumerations;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Enumerations;
    using Basics.Primitives;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// InMemoryIntegrationTransport
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    internal partial class InMemoryIntegrationTransport : IIntegrationTransport,
                                                          IConfigurableIntegrationTransport,
                                                          IExecutableIntegrationTransport,
                                                          IResolvable<IIntegrationTransport>,
                                                          IResolvable<IConfigurableIntegrationTransport>,
                                                          IResolvable<IExecutableIntegrationTransport>
    {
        private readonly AsyncManualResetEvent _ready;
        private readonly MessageQueue<IntegrationMessage> _inputQueue;
        private readonly DeferredQueue<IntegrationMessage> _delayedDeliveryQueue;

        private readonly ILogger _logger;
        private readonly IInMemoryTopology _topology;

        private EnIntegrationTransportStatus _status;

        public InMemoryIntegrationTransport(
            ILogger logger,
            IInMemoryTopology topology)
        {
            _status = EnIntegrationTransportStatus.Stopped;
            _ready = new AsyncManualResetEvent(false);

            _inputQueue = new MessageQueue<IntegrationMessage>();

            var heap = new BinaryHeap<HeapEntry<IntegrationMessage, DateTime>>(EnOrderingDirection.Asc);
            _delayedDeliveryQueue = new DeferredQueue<IntegrationMessage>(heap, PrioritySelector);

            _logger = logger;
            _topology = topology;
        }

        private static DateTime PrioritySelector(IntegrationMessage message)
        {
            return message.ReadRequiredHeader<DeferredUntil>().Value;
        }
    }
}