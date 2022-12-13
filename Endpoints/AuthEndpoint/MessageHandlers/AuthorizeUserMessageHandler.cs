namespace SpaceEngineers.Core.AuthEndpoint.MessageHandlers
{
    using System;
    using System.Linq;
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
    using Microsoft.Extensions.Logging;

    [Component(EnLifestyle.Transient)]
    internal class AuthorizeUserMessageHandler : IMessageHandler<AuthorizeUser>,
                                                 IResolvable<IMessageHandler<AuthorizeUser>>
    {
        private readonly IIntegrationContext _context;
        private readonly IAggregateFactory<User, FindUserSpecification> _findUserAggregateFactory;
        private readonly ILogger _logger;

        public AuthorizeUserMessageHandler(
            IIntegrationContext context,
            IAggregateFactory<User, FindUserSpecification> findUserAggregateFactory,
            ILogger logger)
        {
            _context = context;
            _findUserAggregateFactory = findUserAggregateFactory;
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
                        return Task.FromResult(new UserAuthorizationResult(false));
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

            var requiredFeatures = message
                .RequiredFeatures
                .Select(name => new Feature(name))
                .ToList();

            var accessGranted = user.Authorize(requiredFeatures);

            if (!accessGranted)
            {
                _logger.Warning($"User {message.Username} has no access to activity {message.Activity} that requires permissions for features: {string.Join(", ", message.RequiredFeatures)}");
            }

            return new UserAuthorizationResult(accessGranted);
        }
    }
}