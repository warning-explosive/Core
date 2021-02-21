namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Core.GenericEndpoint;
    using Core.GenericEndpoint.Abstractions;
    using Core.GenericEndpoint.Contract.Abstractions;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class IntegrationMessageFactory : IIntegrationMessageFactory
    {
        private readonly IReadOnlyCollection<IMessageHeaderProvider> _providers;

        public IntegrationMessageFactory(IEnumerable<IMessageHeaderProvider> providers)
        {
            _providers = providers.ToList();
        }

        public IntegrationMessage CreateGeneralMessage<TMessage>(
            TMessage messageToSend,
            EndpointIdentity? endpointIdentity,
            IntegrationMessage? initiatorMessage)
            where TMessage : IIntegrationMessage
        {
            var message = new IntegrationMessage(messageToSend, typeof(TMessage));

            if (endpointIdentity != null)
            {
                message.Headers[IntegratedMessageHeader.SentFrom] = endpointIdentity;
            }

            return ForwardHeaders(message, initiatorMessage);
        }

        private IntegrationMessage ForwardHeaders(IntegrationMessage messageToSend, IntegrationMessage? initiatorMessage)
        {
            if (initiatorMessage == null)
            {
                return messageToSend;
            }

            foreach (var header in _providers.SelectMany(p => p.ForAutomaticForwarding))
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