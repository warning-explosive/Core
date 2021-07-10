namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System;
    using System.Linq;
    using Contract.Abstractions;

    internal class MessageHandlerDoesNotProduceMessageTestCase<TMessage> : ITestCase
        where TMessage : IIntegrationMessage
    {
        private const string Format = "Message handler doesn't have to {0} any {1} of type {2}";

        public string? Assert(TestIntegrationContext integrationContext, Exception? exception)
        {
            return !integrationContext.Messages.OfType<TMessage>().Any()
                ? null
                : BuildErrorMessage();
        }

        private static string BuildErrorMessage()
        {
            var info = MessageInfo.Prepare<TMessage>();
            return string.Format(Format, info.Operation, info.MessageKind, typeof(TMessage).FullName);
        }
    }
}