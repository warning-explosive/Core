namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        public void Send<TCommand>(TCommand command)
            where TCommand : IIntegrationCommand
        {
            Collect(command);
        }

        /// <inheritdoc />
        public void Delay<TCommand>(TCommand command, TimeSpan dueTime)
            where TCommand : IIntegrationCommand
        {
            CollectDelayed(command, DateTime.UtcNow + dueTime);
        }

        /// <inheritdoc />
        public void Delay<TCommand>(TCommand command, DateTime dateTime)
            where TCommand : IIntegrationCommand
        {
            CollectDelayed(command, dateTime.ToUniversalTime());
        }

        /// <inheritdoc />
        public void Publish<TEvent>(TEvent integrationEvent)
            where TEvent : IIntegrationEvent
        {
            Collect(integrationEvent);
        }

        /// <inheritdoc />
        public void Request<TRequest, TReply>(TRequest request)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply
        {
            Collect(request);
        }

        /// <inheritdoc />
        public void Reply<TRequest, TReply>(TRequest request, TReply reply)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply
        {
            Collect(reply);
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