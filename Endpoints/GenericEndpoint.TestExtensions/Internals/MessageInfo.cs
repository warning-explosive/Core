namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System;
    using Contract.Abstractions;
    using Contract.Extensions;

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
            if (typeof(T).IsCommand())
            {
                return new MessageInfo("send", "commands");
            }

            if (typeof(T).IsRequest())
            {
                return new MessageInfo("perform", "requests");
            }

            if (typeof(T).IsReply())
            {
                return new MessageInfo("produce", "replies");
            }

            if (typeof(T).IsEvent())
            {
                return new MessageInfo("publish", "events");
            }

            throw new InvalidOperationException($"Unsupported message type {typeof(T)}");
        }
    }
}