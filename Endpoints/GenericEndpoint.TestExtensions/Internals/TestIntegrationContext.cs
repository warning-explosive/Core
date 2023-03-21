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
        private readonly List<(IIntegrationMessage, DateTime)> _delayedMessages;

        /// <summary> .cctor </summary>
        public TestIntegrationContext()
        {
            _messages = new List<IIntegrationMessage>();
            _delayedMessages = new List<(IIntegrationMessage, DateTime)>();
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
        public IReadOnlyCollection<(IIntegrationMessage, DateTime)> DelayedMessages
        {
            get
            {
                lock (_delayedMessages)
                {
                    return _delayedMessages.ToList();
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
        public Task Delay<TCommand>(TCommand command, TimeSpan dueTime, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return CollectDelayed(command, DateTime.UtcNow + dueTime);
        }

        /// <inheritdoc />
        public Task Delay<TCommand>(TCommand command, DateTime dateTime, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return CollectDelayed(command, dateTime.ToUniversalTime());
        }

        /// <inheritdoc />
        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            return Collect(integrationEvent);
        }

        /// <inheritdoc />
        public Task Request<TRequest, TReply>(TRequest request, CancellationToken token)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply
        {
            return Collect(request);
        }

        /// <inheritdoc />
        public Task Reply<TRequest, TReply>(TRequest request, TReply reply, CancellationToken token)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply
        {
            return Collect(reply);
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

        private Task CollectDelayed<TMessage>(TMessage message, DateTime dateTime)
            where TMessage : IIntegrationMessage
        {
            lock (_delayedMessages)
            {
                _delayedMessages.Add((message, dateTime));
            }

            return Task.CompletedTask;
        }
    }
}