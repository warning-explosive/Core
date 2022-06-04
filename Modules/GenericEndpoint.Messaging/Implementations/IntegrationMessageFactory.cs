namespace SpaceEngineers.Core.GenericEndpoint.Messaging.Implementations
{
    using System;
    using System.Collections.Generic;
    using Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract;
    using Contract.Abstractions;
    using MessageHeaders;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationMessageFactory : IIntegrationMessageFactory,
                                               IResolvable<IIntegrationMessageFactory>
    {
        private readonly IEnumerable<IUserScopeProvider> _userScopeProviders;

        public IntegrationMessageFactory(IEnumerable<IUserScopeProvider> userScopeProviders)
        {
            _userScopeProviders = userScopeProviders;
        }

        public IntegrationMessage CreateGeneralMessage<TMessage>(
            TMessage payload,
            EndpointIdentity endpointIdentity,
            IntegrationMessage? initiatorMessage)
            where TMessage : IIntegrationMessage
        {
            var generalMessage = new IntegrationMessage(payload, typeof(TMessage));

            generalMessage.WriteHeader(new SentFrom(endpointIdentity));

            var user = default(string?);

            foreach (var provider in _userScopeProviders)
            {
                if (provider.TryGetUser(initiatorMessage, out user))
                {
                    break;
                }
            }

            if (user == null || string.IsNullOrEmpty(user))
            {
                throw new InvalidOperationException("Unable to find user scope");
            }

            generalMessage.WriteHeader(new User(user));

            if (initiatorMessage != null)
            {
                var conversationId = initiatorMessage.ReadRequiredHeader<ConversationId>().Value;
                generalMessage.WriteHeader(new ConversationId(conversationId));
                generalMessage.WriteHeader(new InitiatorMessageId(initiatorMessage.ReadRequiredHeader<Id>().Value));
            }
            else
            {
                generalMessage.WriteHeader(new ConversationId(Guid.NewGuid()));
            }

            return generalMessage;
        }
    }
}