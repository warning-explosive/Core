namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using Contract.Abstractions;

    [Unregistered]
    internal class TestIntegrationContext : IIntegrationContext
    {
        private List<IIntegrationMessage> _messages;

        public TestIntegrationContext()
        {
            _messages = new List<IIntegrationMessage>();
        }

        internal IReadOnlyCollection<IIntegrationMessage> Messages
        {
            get
            {
                lock (_messages)
                {
                    return _messages.ToList();
                }
            }
        }

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return Grab(command);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            return Grab(integrationEvent);
        }

        public Task Request<TQuery, TReply>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            return Grab(query);
        }

        public Task Reply<TQuery, TReply>(TQuery query, TReply reply, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            return Grab(reply);
        }

        private Task Grab<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            lock (_messages)
            {
                _messages.Add(message);                
            }

            return Task.CompletedTask;
        }
    }
}