namespace SpaceEngineers.Core.AuthEndpoint.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Commands;
    using Domain.Model;
    using DomainEventHandlers;
    using GenericDomain.Api.Abstractions;
    using GenericEndpoint.Api.Abstractions;
    using UserCreated = Contract.Events.UserCreated;

    [Component(EnLifestyle.Transient)]
    internal class CreateUserMessageHandler : IMessageHandler<CreateUser>,
                                              IResolvable<IMessageHandler<CreateUser>>
    {
        private readonly IIntegrationContext _context;
        private readonly IAggregateFactory<User, CreateUserSpecification> _createUserAggregateFactory;

        public CreateUserMessageHandler(
            IIntegrationContext context,
            IAggregateFactory<User, CreateUserSpecification> createUserAggregateFactory)
        {
            _context = context;
            _createUserAggregateFactory = createUserAggregateFactory;
        }

        public async Task Handle(CreateUser message, CancellationToken token)
        {
            _ = await _createUserAggregateFactory
                .Build(new CreateUserSpecification(message.Username, message.Password), token)
                .ConfigureAwait(false);

            await _context
               .Publish(new UserCreated(message.Username), token)
               .ConfigureAwait(false);
        }
    }
}