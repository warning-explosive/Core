namespace SpaceEngineers.Core.GenericEndpoint.Messaging.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Api.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationMessageFactory : IIntegrationMessageFactory
    {
        private readonly IEnumerable<IMessageHeaderProvider> _providers;
        private readonly IStringFormatter _formatter;

        public IntegrationMessageFactory(IEnumerable<IMessageHeaderProvider> providers,
                                         IStringFormatter formatter)
        {
            _providers = providers;
            _formatter = formatter;
        }

        public IntegrationMessage CreateGeneralMessage<TMessage>(TMessage payload, EndpointIdentity? endpointIdentity, IntegrationMessage? initiatorMessage)
            where TMessage : IIntegrationMessage
        {
            var message = new IntegrationMessage(payload, typeof(TMessage), _formatter);

            if (endpointIdentity != null)
            {
                message.Headers[IntegrationMessageHeader.SentFrom] = endpointIdentity;
            }

            if (initiatorMessage != null)
            {
                ForwardHeaders(message, initiatorMessage);
            }

            return message;
        }

        private void ForwardHeaders(IntegrationMessage messageToSend, IntegrationMessage initiatorMessage)
        {
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
        }
    }
}