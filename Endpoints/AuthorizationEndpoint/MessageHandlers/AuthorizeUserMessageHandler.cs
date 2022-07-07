namespace SpaceEngineers.Core.AuthorizationEndpoint.MessageHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Exceptions;
    using Contract.Messages;
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
        private readonly IIntegrationContext _integrationContext;
        private readonly IAggregateFactory<User, FindUserSpecification> _findUserAggregateFactory;
        private readonly ITokenProvider _tokenProvider;
        private readonly ISettingsProvider<AuthorizationSettings> _authorizationSettingsProvider;

        public AuthorizeUserMessageHandler(
            IIntegrationContext integrationContext,
            IAggregateFactory<User, FindUserSpecification> findUserAggregateFactory,
            ITokenProvider tokenProvider,
            ISettingsProvider<AuthorizationSettings> authorizationSettingsProvider)
        {
            _integrationContext = integrationContext;
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

            await _integrationContext
                .Reply(message, reply, token)
                .ConfigureAwait(false);
        }

        private async Task<UserAuthorizationResult> AuthorizeUser(
            AuthorizeUser message,
            CancellationToken token)
        {
            User? user;
            NotFoundException? notFoundException;

            try
            {
                user = await _findUserAggregateFactory
                   .Build(new FindUserSpecification(message.Username), token)
                   .ConfigureAwait(false);

                notFoundException = null;
            }
            catch (NotFoundException exception)
            {
                user = null;
                notFoundException = exception;
            }

            string authorizationToken;
            string resultMessage;

            if (user != null && user.CheckPassword(message.Password))
            {
                var settings = await _authorizationSettingsProvider
                    .Get(token)
                    .ConfigureAwait(false);

                authorizationToken = _tokenProvider.GenerateToken(message.Username, settings.TokenExpirationTimeout);
                resultMessage = string.Empty;
            }
            else if (user == null && notFoundException != null)
            {
                authorizationToken = string.Empty;
                resultMessage = notFoundException.Message;
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