namespace SpaceEngineers.Core.AuthorizationEndpoint.MessageHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Contract;
    using CrossCuttingConcerns.Settings;
    using Domain;
    using GenericDomain.Api.Abstractions;
    using GenericEndpoint.Api.Abstractions;
    using JwtAuthentication;
    using Settings;

    [Component(EnLifestyle.Transient)]
    internal class AuthorizeUserMessageHandler : IMessageHandler<AuthorizeUser>,
                                                 IResolvable<IMessageHandler<AuthorizeUser>>
    {
        private readonly IIntegrationContext _context;
        private readonly IAggregateFactory<User, FindUserSpecification> _findUserAggregateFactory;
        private readonly ITokenProvider _tokenProvider;
        private readonly ISettingsProvider<AuthorizationSettings> _authorizationSettingsProvider;

        public AuthorizeUserMessageHandler(
            IIntegrationContext context,
            IAggregateFactory<User, FindUserSpecification> findUserAggregateFactory,
            ITokenProvider tokenProvider,
            ISettingsProvider<AuthorizationSettings> authorizationSettingsProvider)
        {
            _context = context;
            _findUserAggregateFactory = findUserAggregateFactory;
            _tokenProvider = tokenProvider;
            _authorizationSettingsProvider = authorizationSettingsProvider;
        }

        public async Task Handle(AuthorizeUser message, CancellationToken token)
        {
            var reply = await ExecutionExtensions
                .TryAsync(message, AuthorizeUser)
                .Catch<Exception>()
                .Invoke((exception, _) => Task.FromResult(new UserAuthorizationResult(message.Username, string.Empty, exception.Message)), token)
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

            if (user.CheckPassword(message.Password))
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

            // TODO: #165 - add support for sliding expiration
            return new UserAuthorizationResult(message.Username, authorizationToken, resultMessage);
        }
    }
}