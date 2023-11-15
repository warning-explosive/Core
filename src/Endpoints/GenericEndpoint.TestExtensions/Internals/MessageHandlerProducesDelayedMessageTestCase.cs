namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Basics;
    using Contract.Abstractions;

    internal class MessageHandlerProducesDelayedMessageTestCase<TMessage> : ITestCase
        where TMessage : IIntegrationMessage
    {
        private const string Format = "Message handler has to {0} any {1} of type {2} that corresponds to predicate '{3}'";

        private readonly Expression<Func<TMessage, DateTime, bool>> _predicate;

        internal MessageHandlerProducesDelayedMessageTestCase(Expression<Func<TMessage, DateTime, bool>> predicate)
        {
            _predicate = predicate;
        }

        public string? Assert(ITestIntegrationContext integrationContext, Exception? exception)
        {
            var predicateFunc = _predicate.Compile();

            return integrationContext.DelayedMessages.Where(info => info.Message is TMessage).Any(info => predicateFunc((TMessage)info.Message, info.DateTime))
                ? null
                : BuildErrorMessage();
        }

        private string BuildErrorMessage()
        {
            var info = MessageInfo.Prepare<TMessage>();
            return Format.Format(info.Operation, info.MessageKind, typeof(TMessage).FullName, _predicate);
        }
    }
}