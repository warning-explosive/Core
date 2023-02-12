namespace SpaceEngineers.Core.AuthEndpoint.MessageHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Contract;
    using CrossCuttingConcerns.Logging;
    using Domain;
    using DomainEventHandlers;
    using GenericDomain.Api.Abstractions;
    using GenericEndpoint.Api.Abstractions;
    using JwtAuthentication;
    using Microsoft.Extensions.Logging;
    using Settings;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Transient)]
    internal class AuthenticateUserMessageHandler : IMessageHandler<AuthenticateUser>,
                                                    IResolvable<IMessageHandler<AuthenticateUser>>
    {
        private readonly IIntegrationContext _context;
        private readonly IAggregateFactory<User, FindUserSpecification> _findUserAggregateFactory;
        private readonly ITokenProvider _tokenProvider;
        private readonly ISettingsProvider<AuthorizationSettings> _authorizationSettingsProvider;
        private readonly ILogger _logger;

        public AuthenticateUserMessageHandler(
            IIntegrationContext context,
            IAggregateFactory<User, FindUserSpecification> findUserAggregateFactory,
            ITokenProvider tokenProvider,
            ISettingsProvider<AuthorizationSettings> authorizationSettingsProvider,
            ILogger logger)
        {
            _context = context;
            _findUserAggregateFactory = findUserAggregateFactory;
            _tokenProvider = tokenProvider;
            _authorizationSettingsProvider = authorizationSettingsProvider;
            _logger = logger;
        }

        public async Task Handle(AuthenticateUser message, CancellationToken token)
        {
            var reply = await AuthenticateUser(message, token)
               .TryAsync()
               .Catch<Exception>()
               .Invoke(exception =>
                    {
                        _logger.Error(exception);
                        return new UserAuthenticationResult(message.Username, string.Empty);
                    },
                    token)
               .ConfigureAwait(false);

            await _context
                .Reply(message, reply, token)
                .ConfigureAwait(false);
        }

        private async Task<UserAuthenticationResult> AuthenticateUser(
            AuthenticateUser message,
            CancellationToken token)
        {
            var user = await _findUserAggregateFactory
               .Build(new FindUserSpecification(message.Username), token)
               .ConfigureAwait(false);

            string authenticationToken;

            if (user.CheckPassword(new Password(message.Password)))
            {
                var settings = await _authorizationSettingsProvider
                    .Get(token)
                    .ConfigureAwait(false);

                authenticationToken = user.GenerateAuthorizationToken(AuthorizationTokenGenerator(_tokenProvider, settings));
            }
            else
            {
                authenticationToken = string.Empty;
            }

            // TODO: #201 - add support for sliding expiration
            return new UserAuthenticationResult(message.Username, authenticationToken);

            static Func<Username, IReadOnlyCollection<Feature>, string> AuthorizationTokenGenerator(
                ITokenProvider tokenProvider,
                AuthorizationSettings settings)
            {
                return (username, features) => tokenProvider.GenerateToken(
                    username.Value,
                    features.Select(feature => feature.Name).ToHashSet(StringComparer.OrdinalIgnoreCase),
                    settings.TokenExpirationTimeout);
            }
        }
    }
}