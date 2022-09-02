namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using Basics;
    using Contract.Abstractions;

    internal class MessageInfo
    {
        public MessageInfo(string operation, string messageKind)
        {
            Operation = operation;
            MessageKind = messageKind;
        }

        internal string Operation { get; }

        internal string MessageKind { get; }

        public static MessageInfo Prepare<T>()
            where T : IIntegrationMessage
        {
            if (typeof(IIntegrationCommand).IsAssignableFrom(typeof(T)))
            {
                return new MessageInfo("send", "commands");
            }

            if (typeof(T).IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>)))
            {
                return new MessageInfo("request", "queries");
            }

            if (typeof(IIntegrationEvent).IsAssignableFrom(typeof(T)))
            {
                return new MessageInfo("publish", "events");
            }

            return new MessageInfo("produce", "messages");
        }
    }
}