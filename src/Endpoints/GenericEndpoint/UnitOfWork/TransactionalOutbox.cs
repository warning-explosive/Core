namespace SpaceEngineers.Core.GenericEndpoint.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;

    [Component(EnLifestyle.Scoped)]
    internal class TransactionalOutbox : ITransactionalOutbox,
                                         IResolvable<ITransactionalOutbox>,
                                         IDisposable
    {
        private readonly IIntegrationTransport _transport;

        private readonly List<IntegrationMessage> _outgoingMessages;

        public TransactionalOutbox(IIntegrationTransport transport)
        {
            _transport = transport;

            _outgoingMessages = new List<IntegrationMessage>();
        }

        public void Dispose()
        {
            _outgoingMessages.Clear();
        }

        public void Add(IntegrationMessage message)
        {
            _outgoingMessages.Add(message);
        }

        public IReadOnlyCollection<IntegrationMessage> All()
        {
            return _outgoingMessages;
        }

        public async Task DeliverMessages(CancellationToken token)
        {
            foreach (var message in _outgoingMessages)
            {
                _ = await _transport
                    .Enqueue(message, token)
                    .ConfigureAwait(false);
            }
        }
    }
}