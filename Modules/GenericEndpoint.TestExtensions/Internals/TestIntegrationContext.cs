namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Contract.Abstractions;

    /// <summary>
    /// TestIntegrationContext
    /// </summary>
    [UnregisteredComponent]
    public class TestIntegrationContext : ITestIntegrationContext,
                                          IIntegrationContext,
                                          IResolvable<IIntegrationContext>
    {
        private readonly List<IIntegrationMessage> _messages;

        /// <summary> .cctor </summary>
        public TestIntegrationContext()
        {
            _messages = new List<IIntegrationMessage>();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IIntegrationMessage> Messages
        {
            get
            {
                lock (_messages)
                {
                    return _messages.ToList();
                }
            }
        }

        /// <inheritdoc />
        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return Collect(command);
        }

        /// <inheritdoc />
        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            return Collect(integrationEvent);
        }

        /// <inheritdoc />
        public Task Request<TQuery, TReply>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationReply
        {
            return Collect(query);
        }

        /// <inheritdoc />
        public Task Reply<TQuery, TReply>(TQuery query, TReply reply, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationReply
        {
            return Collect(reply);
        }

        /// <inheritdoc />
        public Task<TReply> RpcRequest<TQuery, TReply>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationReply
        {
            throw new NotSupportedException("Rpc request");
        }

        private Task Collect<TMessage>(TMessage message)
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