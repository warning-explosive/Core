namespace SpaceEngineers.Core.AuthEndpoint.MessageHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Contract.Queries;
    using Contract.Replies;
    using CrossCuttingConcerns.Extensions;
    using Domain.Model;
    using DomainEventHandlers;
    using GenericDomain.Api.Abstractions;
    using GenericEndpoint.Api.Abstractions;
    using JwtAuthentication;
    using Microsoft.Extensions.Logging;
    using Settings;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Transient)]
    internal class AuthorizeUserMessageHandler : IMessageHandler<AuthorizeUser>,
                                                 IResolvable<IMessageHandler<AuthorizeUser>>
    {
        private readonly IIntegrationContext _context;
        private readonly IAggregateFactory<User, FindUserSpecification> _findUserAggregateFactory;
        private readonly ITokenProvider _tokenProvider;
        private readonly ISettingsProvider<AuthorizationSettings> _authorizationSettingsProvider;
        private readonly ILogger _logger;

        public AuthorizeUserMessageHandler(
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

        public async Task Handle(AuthorizeUser message, CancellationToken token)
        {
            var reply = await ExecutionExtensions
               .TryAsync(message, AuthorizeUser)
               .Catch<Exception>()
               .Invoke((exception, _) =>
                    {
                        _logger.Error(exception);
                        return Task.FromResult(new UserAuthorizationResult(message.Username, string.Empty, exception.Message));
                    },
                    token)
               .ConfigureAwait(false);

            await _context
                .Reply(message, reply, token)
                .ConfigureAwait(false);
        }

        private async Task<UserAuthorizationResult> AuthorizeUser(
            AuthorizeUser message,
            CancellationToken token)
        {
            var user = await _findUserAggregateFactory
               .Build(new FindUserSpecification(message.Username), token)
               .ConfigureAwait(false);

            string authorizationToken;
            string resultMessage;

            if (user.CheckPassword(new Password(message.Password)))
            {
                var settings = await _authorizationSettingsProvider
                    .Get(token)
                    .ConfigureAwait(false);

                authorizationToken = _tokenProvider.GenerateToken(message.Username, settings.TokenExpirationTimeout);
                resultMessage = string.Empty;
            }
            else
            {
                authorizationToken = string.Empty;
                resultMessage = "Wrong password";
            }

            // TODO: #201 - add support for sliding expiration
            return new UserAuthorizationResult(message.Username, authorizationToken, resultMessage);
        }
    }
}