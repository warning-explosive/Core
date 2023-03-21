namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System;
    using System.Linq;
    using Basics;
    using Contract.Abstractions;

    internal class MessageHandlerDoesNotProduceDelayedMessageTestCase<TMessage> : ITestCase
        where TMessage : IIntegrationMessage
    {
        private const string Format = "Message handler doesn't have to {0} any {1} of type {2}";

        public string? Assert(ITestIntegrationContext integrationContext, Exception? exception)
        {
            return !integrationContext.DelayedMessages.Select(info => info.Message).OfType<TMessage>().Any()
                ? null
                : BuildErrorMessage();
        }

        private static string BuildErrorMessage()
        {
            var info = MessageInfo.Prepare<TMessage>();
            return Format.Format(info.Operation, info.MessageKind, typeof(TMessage).FullName);
        }
    }
}