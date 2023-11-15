namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Abstractions;
    using MessageHeaders;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationMessageFactory : IIntegrationMessageFactory,
                                               IResolvable<IIntegrationMessageFactory>
    {
        private readonly IEnumerable<IIntegrationMessageHeaderProvider> _providers;

        public IntegrationMessageFactory(IEnumerable<IIntegrationMessageHeaderProvider> providers)
        {
            _providers = providers;
        }

        public IntegrationMessage CreateGeneralMessage(
            IIntegrationMessage payload,
            Type reflectedType,
            IReadOnlyCollection<IIntegrationMessageHeader> headers,
            IntegrationMessage? initiatorMessage)
        {
            var generalMessage = new IntegrationMessage(payload, reflectedType);

            foreach (var provider in _providers)
            {
                provider.WriteHeaders(generalMessage, initiatorMessage);
            }

            foreach (var header in headers)
            {
                generalMessage.OverwriteHeader(header);
            }

            return generalMessage;
        }
    }
}