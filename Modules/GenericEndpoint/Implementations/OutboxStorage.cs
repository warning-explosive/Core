namespace SpaceEngineers.Core.GenericEndpoint.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Messaging;

    [Component(EnLifestyle.Scoped)]
    internal class OutboxStorage : IOutboxStorage
    {
        private readonly IDictionary<Guid, IntegrationMessage> _outgoingMessages;

        public OutboxStorage()
        {
            _outgoingMessages = new Dictionary<Guid, IntegrationMessage>();
        }

        public IReadOnlyCollection<IntegrationMessage> All
        {
            get
            {
                lock (_outgoingMessages)
                {
                    return _outgoingMessages.Values.ToList();
                }
            }
        }

        public Task Add(IntegrationMessage message, CancellationToken token)
        {
            lock (_outgoingMessages)
            {
                _outgoingMessages.Add(message.Id, message);
            }

            return Task.CompletedTask;
        }

        public Task Ack(IntegrationMessage message, CancellationToken token)
        {
            lock (_outgoingMessages)
            {
                _outgoingMessages.Remove(message.Id);
            }

            return Task.CompletedTask;
        }

        public Task Purge(CancellationToken token)
        {
            lock (_outgoingMessages)
            {
                _outgoingMessages.Clear();
            }

            return Task.CompletedTask;
        }
    }
}