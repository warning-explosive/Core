namespace SpaceEngineers.Core.StatisticsEndpoint.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CrossCuttingConcerns.Api.Abstractions;
    using GenericDomain;
    using GenericEndpoint.Messaging;

    internal class MessageInfo : EntityBase
    {
        public MessageInfo(
            string message,
            Type reflectedType,
            IReadOnlyCollection<MessageHeader> messageHeaders)
        {
            Message = message;
            ReflectedType = reflectedType.FullName;
            MessageHeaders = messageHeaders;
        }

        public string Message { get; }

        public string ReflectedType { get; }

        public IReadOnlyCollection<MessageHeader> MessageHeaders { get; }

        public static MessageInfo FromIntegrationMessage(IntegrationMessage message, IJsonSerializer serializer)
        {
            var headers = message
                .Headers
                .Select(it => new MessageHeader(it.Key, serializer.SerializeObject(it.Value), it.Value?.GetType()))
                .ToList();

            return new MessageInfo(serializer.SerializeObject(message), message.ReflectedType, headers);
        }
    }
}