namespace SpaceEngineers.Core.AuthorizationEndpoint.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Messages;
    using GenericEndpoint.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class AuthorizeUserMessageHandler : IMessageHandler<AuthorizeUser>,
                                                 ICollectionResolvable<IMessageHandler<AuthorizeUser>>
    {
        private readonly IIntegrationContext _integrationContext;

        public AuthorizeUserMessageHandler(IIntegrationContext integrationContext)
        {
            _integrationContext = integrationContext;
        }

        public Task Handle(AuthorizeUser message, CancellationToken token)
        {
            // TODO: #165 - authenticate user
            // TODO: #165 - generate token for successfully authenticated user
            // TODO: #165 - add support for sliding expiration
            var reply = new UserAuthorizationResult(message.Username, false, "unauthorized");

            return _integrationContext.Reply(message, reply, token);
        }
    }
}