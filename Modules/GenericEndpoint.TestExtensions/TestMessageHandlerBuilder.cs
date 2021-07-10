namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using Api.Abstractions;
    using Basics;
    using Contract.Abstractions;
    using Internals;

    /// <summary>
    /// Test message handler builder
    /// </summary>
    /// <typeparam name="TMessage">TMessage type-argument</typeparam>
    public class TestMessageHandlerBuilder<TMessage>
        where TMessage : IIntegrationMessage
    {
        private readonly TMessage _message;
        private readonly IMessageHandler<TMessage> _messageHandler;
        private readonly ICollection<ITestCase> _testCases;

        internal TestMessageHandlerBuilder(TMessage message, IMessageHandler<TMessage> messageHandler)
        {
            _message = message;
            _messageHandler = messageHandler;
            _testCases = new List<ITestCase>();
        }

        /// <summary>
        /// Invokes message handler and applies test assertions
        /// </summary>
        public void Invoke()
        {
            var context = new TestIntegrationContext();

            var exception = ExecutionExtensions
                .Try(() =>
                {
                    _messageHandler.Handle(_message, context, CancellationToken.None).Wait();
                    return (Exception?)null;
                })
                .Catch<Exception>()
                .Invoke(ex => ex);

            var errors = _testCases
                .Select(testCase => testCase.Assert(context, exception))
                .Where(errorMessage => errorMessage != null)
                .Select(errorMessage => new InvalidOperationException(errorMessage))
                .ToList();

            switch (errors.Count)
            {
                case > 1: throw new AggregateException(errors);
                case 1:
                {
                    var error = errors.Single();
                    error.Rethrow();
                    throw error;
                }
            }
        }

        /// <summary>
        /// Validates that message handler sends command that corresponds to specified predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> Sends<TCommand>(Expression<Func<TCommand, bool>> predicate)
            where TCommand : IIntegrationCommand
        {
            _testCases.Add(new MessageHandlerProducesMessageTestCase<TCommand>(predicate));
            return this;
        }

        /// <summary>
        /// Validates that message handler doesn't send any commands with specified type
        /// </summary>
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> DoesNotSend<TCommand>()
            where TCommand : IIntegrationCommand
        {
            _testCases.Add(new MessageHandlerDoesNotProduceMessageTestCase<TCommand>());
            return this;
        }

        /// <summary>
        /// Validates that message handler publishes event that corresponds to specified predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <typeparam name="TEvent">TEvent type-argument</typeparam>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> Publishes<TEvent>(Expression<Func<TEvent, bool>> predicate)
            where TEvent : IIntegrationEvent
        {
            _testCases.Add(new MessageHandlerProducesMessageTestCase<TEvent>(predicate));
            return this;
        }

        /// <summary>
        /// Validates that message handler doesn't publish any events with specified type
        /// </summary>
        /// <typeparam name="TEvent">TEvent type-argument</typeparam>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> DoesNotPublish<TEvent>()
            where TEvent : IIntegrationEvent
        {
            _testCases.Add(new MessageHandlerDoesNotProduceMessageTestCase<TEvent>());
            return this;
        }

        /// <summary>
        /// Validates that message handler requests query that corresponds to specified predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <typeparam name="TQuery">TQuery type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> Requests<TQuery, TReply>(Expression<Func<TQuery, bool>> predicate)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            _testCases.Add(new MessageHandlerProducesMessageTestCase<TQuery>(predicate));
            return this;
        }

        /// <summary>
        /// Validates that message handler doesn't request any queries with specified type
        /// </summary>
        /// <typeparam name="TQuery">TQuery type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> DoesNotRequest<TQuery, TReply>()
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage
        {
            _testCases.Add(new MessageHandlerDoesNotProduceMessageTestCase<TQuery>());
            return this;
        }

        /// <summary>
        /// Validates that message handler replies with message that corresponds to specified predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> Replies<TReply>(Expression<Func<TReply, bool>> predicate)
            where TReply : IIntegrationMessage
        {
            _testCases.Add(new MessageHandlerProducesMessageTestCase<TReply>(predicate));
            return this;
        }

        /// <summary>
        /// Validates that message handler doesn't reply on incoming query with specified reply message type
        /// </summary>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> DoesNotReply<TReply>()
            where TReply : IIntegrationMessage
        {
            _testCases.Add(new MessageHandlerDoesNotProduceMessageTestCase<TReply>());
            return this;
        }

        /// <summary>
        /// Validates that message handler doesn't produce any outgoing messages
        /// </summary>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> ProducesNothing()
        {
            _testCases.Add(new MessageHandlerDoesNotProduceMessageTestCase<TMessage>());
            return this;
        }

        /// <summary>
        /// Validates that message handler throws an exception
        /// </summary>
        /// <typeparam name="TException">TException type-argument</typeparam>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> Throws<TException>()
            where TException : Exception
        {
            _testCases.Add(new MessageHandlerThrowsExceptionTestCase<TException>(_ => true));
            return this;
        }

        /// <summary>
        /// Validates that message handler throws an exception
        /// </summary>
        /// <param name="assertion">Assertion</param>
        /// <typeparam name="TException">TException type-argument</typeparam>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> Throws<TException>(Func<TException, bool> assertion)
            where TException : Exception
        {
            _testCases.Add(new MessageHandlerThrowsExceptionTestCase<TException>(assertion));
            return this;
        }

        /// <summary>
        /// Validates that message handler doesn't throw an exception
        /// </summary>
        /// <typeparam name="TException">TException type-argument</typeparam>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> DoesNotThrow<TException>()
            where TException : Exception
        {
            _testCases.Add(new MessageHandlerDoesNotThrowExceptionTestCase<TException>());
            return this;
        }

        /// <summary>
        /// Validates that message handler doesn't throw any exception
        /// </summary>
        /// <returns>TestMessageHandlerBuilder</returns>
        public TestMessageHandlerBuilder<TMessage> DoesNotThrow()
        {
            _testCases.Add(new MessageHandlerDoesNotThrowExceptionTestCase<Exception>());
            return this;
        }
    }
}