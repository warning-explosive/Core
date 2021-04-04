namespace SpaceEngineers.Core.GenericHost.InMemoryIntegrationTransport.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationMessageFactory : IIntegrationMessageFactory
    {
        private readonly IEnumerable<IMessageHeaderProvider> _providers;

        public IntegrationMessageFactory(IEnumerable<IMessageHeaderProvider> providers)
        {
            _providers = providers;
        }

        public IntegrationMessage CreateGeneralMessage<TMessage>(TMessage payload, EndpointIdentity? endpointIdentity, IntegrationMessage? initiatorMessage)
            where TMessage : IIntegrationMessage
        {
            var message = new IntegrationMessage(payload, typeof(TMessage));

            if (endpointIdentity != null)
            {
                message.Headers[IntegratedMessageHeader.SentFrom] = endpointIdentity;
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