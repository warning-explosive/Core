namespace SpaceEngineers.Core.AuthorizationEndpoint.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract.Messages;
    using DataAccess.Api.Transaction;
    using Domain;
    using GenericDomain.Api.Abstractions;
    using GenericEndpoint.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class CreateUserMessageHandler : IMessageHandler<CreateUser>,
                                              IResolvable<IMessageHandler<CreateUser>>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IAggregateFactory<User, CreateUserSpecification> _createUserAggregateFactory;

        public CreateUserMessageHandler(
            IDatabaseContext databaseContext,
            IAggregateFactory<User, CreateUserSpecification> createUserAggregateFactory)
        {
            _databaseContext = databaseContext;
            _createUserAggregateFactory = createUserAggregateFactory;
        }

        public async Task Handle(CreateUser message, CancellationToken token)
        {
            var user = await _createUserAggregateFactory
                .Build(new CreateUserSpecification(message.Username, message.Password), token)
                .ConfigureAwait(false);

            await _databaseContext
                .Track(user, token)
                .ConfigureAwait(false);
        }
    }
}