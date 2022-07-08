namespace SpaceEngineers.Core.AuthorizationEndpoint.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Messages;
    using Domain;
    using GenericDomain.Api.Abstractions;
    using GenericEndpoint.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class CreateUserMessageHandler : IMessageHandler<CreateUser>,
                                              IResolvable<IMessageHandler<CreateUser>>
    {
        private readonly IAggregateFactory<User, CreateUserSpecification> _createUserAggregateFactory;

        public CreateUserMessageHandler(IAggregateFactory<User, CreateUserSpecification> createUserAggregateFactory)
        {
            _createUserAggregateFactory = createUserAggregateFactory;
        }

        public async Task Handle(CreateUser message, CancellationToken token)
        {
            _ = await _createUserAggregateFactory
                .Build(new CreateUserSpecification(message.Username, message.Password), token)
                .ConfigureAwait(false);
        }
    }
}