namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Contract.Abstractions;

    internal class MessageHandlerProducesMessageTestCase<TMessage> : ITestCase
        where TMessage : IIntegrationMessage
    {
        private const string Format = "Message handler has to {0} any {1} of type {2} that corresponds to predicate '{3}'";

        private readonly Expression<Func<TMessage, bool>> _predicate;

        internal MessageHandlerProducesMessageTestCase(Expression<Func<TMessage, bool>> predicate)
        {
            _predicate = predicate;
        }

        public string? Assert(TestIntegrationContext integrationContext, Exception? exception)
        {
            var predicateFunc = _predicate.Compile();

            return integrationContext.Messages.OfType<TMessage>().Any(predicateFunc)
                ? null
                : BuildErrorMessage();
        }

        private string BuildErrorMessage()
        {
            var info = MessageInfo.Prepare<TMessage>();
            return string.Format(Format, info.Operation, info.MessageKind, typeof(TMessage).FullName, _predicate);
        }
    }
}