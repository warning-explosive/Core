namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using Abstractions;
    using Basics;
    using Contract.Abstractions;
    using Internals;

    /// <summary>
    /// Message handler test builder
    /// </summary>
    /// <typeparam name="TMessage">TMessage type-argument</typeparam>
    public class MessageHandlerTestBuilder<TMessage>
        where TMessage : IIntegrationMessage
    {
        private const string RequiredFormat = "Message handler hasn't {0} any {1} or type {2} that corresponds to predicate '{3}'";
        private const string ShouldNotProduceFormat = "Message handler should not {0} any {1} of type {2}";
        
        private readonly TMessage _message;
        private readonly IMessageHandler<TMessage> _messageHandler;
        private readonly ICollection<TestCase> _testCases;

        internal MessageHandlerTestBuilder(TMessage message, IMessageHandler<TMessage> messageHandler)
        {
            _message = message;
            _messageHandler = messageHandler;
            _testCases = new List<TestCase>();
        }

        /// <summary>
        /// Invokes message handler and applies test assertions
        /// </summary>
        public void Invoke()
        {
            var context = new TestIntegrationContext();

            _messageHandler.Handle(_message, context, CancellationToken.None).Wait();

            var errors = _testCases
                .Where(testCase => !testCase.Assertion.Invoke(context.Messages))
                .Select(testCase => new InvalidOperationException(testCase.ErrorMessage))
                .ToList();

            switch (errors.Count)
            {
                case > 1: throw new AggregateException(errors);
                case 1: throw errors.Single();
            }
        }

        /// <summary>
        /// Validates the message handler has sent command that corresponds to specified predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        /// <returns>MessageHandlerTestBuilder</returns>
        public MessageHandlerTestBuilder<TMessage> Sent<TCommand>(Expression<Func<TCommand, bool>> predicate)
            where TCommand : IIntegrationCommand
        {
            RegisterRequiredTestCase(predicate);
            return this;
        }

        /// <summary>
        /// Validates the message handler should not send any commands with specified type
        /// </summary>
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        /// <returns>MessageHandlerTestBuilder</returns>
        public MessageHandlerTestBuilder<TMessage> ShouldNotSend<TCommand>()
            where TCommand : IIntegrationCommand
        {
            RegisterNegativeTestCase<TCommand>();
            return this;
        }

        /// <summary>
        /// Validates the message handler has published event that corresponds to specified predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <typeparam name="TEvent">TEvent type-argument</typeparam>
        /// <returns>MessageHandlerTestBuilder</returns>
        public MessageHandlerTestBuilder<TMessage> Published<TEvent>(Expression<Func<TEvent, bool>> predicate)
            where TEvent : IIntegrationEvent
        {
            RegisterRequiredTestCase(predicate);
            return this;
        }

        /// <summary>
        /// Validates the message handler should not publish any events with specified type
        /// </summary>
        /// <typeparam name="TEvent">TEvent type-argument</typeparam>
        /// <returns>MessageHandlerTestBuilder</returns>
        public MessageHandlerTestBuilder<TMessage> ShouldNotPublish<TEvent>()
            where TEvent : IIntegrationEvent
        {
            RegisterNegativeTestCase<TEvent>();
            return this;
        }

        /// <summary>
        /// Validates the message handler has requested query that corresponds to specified predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <typeparam name="TQuery">TQuery type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>MessageHandlerTestBuilder</returns>
        public MessageHandlerTestBuilder<TMessage> Requested<TQuery, TReply>(Expression<Func<TQuery, bool>> predicate)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            RegisterRequiredTestCase(predicate);
            return this;
        }


        /// <summary>
        /// Validates the message handler should not request any queries with specified type
        /// </summary>
        /// <typeparam name="TQuery">TQuery type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>MessageHandlerTestBuilder</returns>
        public MessageHandlerTestBuilder<TMessage> ShouldNotRequest<TQuery, TReply>()
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            RegisterNegativeTestCase<TQuery>();
            return this;
        }

        /// <summary>
        /// Validates the message handler has replied with message that corresponds to specified predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>MessageHandlerTestBuilder</returns>
        public MessageHandlerTestBuilder<TMessage> Replied<TReply>(Expression<Func<TReply, bool>> predicate)
            where TReply : IIntegrationMessage
        {
            RegisterRequiredTestCase(predicate);
            return this;
        }

        /// <summary>
        /// Validates the message handler should not reply on incoming query with specified reply message type
        /// </summary>
        /// <returns>MessageHandlerTestBuilder</returns>
        public MessageHandlerTestBuilder<TMessage> ShouldNotReply<TReply>()
            where TReply : IIntegrationMessage
        {
            RegisterNegativeTestCase<TReply>();
            return this;
        }

        /// <summary>
        /// Validates the message handler should not produce any outgoing messages
        /// </summary>
        /// <returns>MessageHandlerTestBuilder</returns>
        public MessageHandlerTestBuilder<TMessage> ShouldProduceNothing()
        {
            RegisterNegativeTestCase<IIntegrationMessage>();
            return this;
        }

        private void RegisterRequiredTestCase<T>(Expression<Func<T, bool>> predicate)
            where T : IIntegrationMessage
        {
            var predicateFunc = predicate.Compile();
            var errorMessage = RequiredMessage(predicate);
            _testCases.Add(new TestCase(messages => messages.OfType<T>().Any(predicateFunc), errorMessage));
        }

        private void RegisterNegativeTestCase<T>()
            where T : IIntegrationMessage
        {
            var errorMessage = ShouldNotProduceMessage<T>();
            _testCases.Add(new TestCase(messages => !messages.OfType<T>().Any(), errorMessage));
        }

        private string RequiredMessage<T>(Expression<Func<T, bool>> predicate)
            where T : IIntegrationMessage
        {
            var info = GetMessageInfo<T>();
            return string.Format(RequiredFormat, info.OperationV3, info.MessageKind, typeof(T).FullName, predicate);
        }

        private string ShouldNotProduceMessage<T>()
            where T : IIntegrationMessage
        {
            var info = GetMessageInfo<T>();
            return string.Format(ShouldNotProduceFormat, info.Operation, info.MessageKind, typeof(T).FullName);
        }

        private static MessageInfo GetMessageInfo<T>()
            where T : IIntegrationMessage
        {
            if (typeof(IIntegrationCommand).IsAssignableFrom(typeof(T)))
            {
                return new MessageInfo("send", "sent", "commands");
            }

            if (typeof(T).IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>)))
            {
                return new MessageInfo("request", "requested", "queries");
            }

            if (typeof(IIntegrationEvent).IsAssignableFrom(typeof(T)))
            {
                return new MessageInfo("publish", "published", "events");
            }

            return new MessageInfo("produce", "produced", "messages");
        }

        private class TestCase
        {
            internal TestCase(Func<IReadOnlyCollection<IIntegrationMessage>, bool> assertion, string errorMessage)
            {
                Assertion = assertion;
                ErrorMessage = errorMessage;
            }

            internal Func<IReadOnlyCollection<IIntegrationMessage>, bool> Assertion { get; }

            internal string ErrorMessage { get; }
        }

        private class MessageInfo
        {
            public MessageInfo(string operation, string operationV3, string messageKind)
            {
                Operation = operation;
                OperationV3 = operationV3;
                MessageKind = messageKind;
            }

            internal string Operation { get; }

            internal string OperationV3 { get; }

            internal string MessageKind { get; }
        }
    }
}