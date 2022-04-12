namespace SpaceEngineers.Core.GenericEndpoint.UnitOfWork
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Messaging;

    [Component(EnLifestyle.Scoped)]
    internal class OutboxStorage : IOutboxStorage,
                                   IResolvable<IOutboxStorage>
    {
        private readonly ICollection<IntegrationMessage> _outgoingMessages;

        public OutboxStorage()
        {
            _outgoingMessages = new List<IntegrationMessage>();
        }

        public Task Add(IntegrationMessage message, CancellationToken token)
        {
            lock (_outgoingMessages)
            {
                _outgoingMessages.Add(message);
            }

            return Task.CompletedTask;
        }

        public IReadOnlyCollection<IntegrationMessage> All()
        {
            lock (_outgoingMessages)
            {
                return _outgoingMessages.ToList();
            }
        }
    }
}