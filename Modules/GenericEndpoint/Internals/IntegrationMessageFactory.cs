namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Contract.Abstractions;
    using GenericEndpoint;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class IntegrationMessageFactory : IIntegrationMessageFactory
    {
        private readonly IReadOnlyCollection<IMessageHeaderProvider> _providers;

        public IntegrationMessageFactory(IEnumerable<IMessageHeaderProvider> providers)
        {
            _providers = providers.ToList();
        }

        public IntegrationMessage CreateGeneralMessage<TMessage>(TMessage payload, EndpointScope? endpointScope)
            where TMessage : IIntegrationMessage
        {
            var message = new IntegrationMessage(payload, typeof(TMessage));

            if (endpointScope == null)
            {
                return message;
            }

            message.Headers[IntegratedMessageHeader.SentFrom] = endpointScope.Identity;

            return ForwardHeaders(message, endpointScope.InitiatorMessage);
        }

        private IntegrationMessage ForwardHeaders(IntegrationMessage messageToSend, IntegrationMessage? initiatorMessage)
        {
            if (initiatorMessage == null)
            {
                return messageToSend;
            }

            var headersToForward = _providers
                .SelectMany(p => p.ForAutomaticForwarding)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var header in headersToForward)
            {
                if (initiatorMessage.Headers.TryGetValue(header, out var value))
                {
                    messageToSend.Headers[header] = value;
                }
            }

            return messageToSend;
        }
    }
}