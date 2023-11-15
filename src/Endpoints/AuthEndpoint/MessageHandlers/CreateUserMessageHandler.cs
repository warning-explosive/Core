namespace SpaceEngineers.Core.AuthEndpoint.MessageHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Contract;
    using DataAccess.Orm.Sql.Linq;
    using DataAccess.Orm.Sql.Transaction;
    using Domain;
    using DomainEventHandlers;
    using GenericDomain.Api.Abstractions;
    using GenericEndpoint.Api.Abstractions;
    using UserWasCreated = Contract.UserWasCreated;

    [Component(EnLifestyle.Transient)]
    internal class CreateUserMessageHandler : IMessageHandler<CreateUser>,
                                              IResolvable<IMessageHandler<CreateUser>>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IIntegrationContext _context;
        private readonly IAggregateFactory<User, CreateUserSpecification> _createUserAggregateFactory;

        public CreateUserMessageHandler(
            IDatabaseContext databaseContext,
            IIntegrationContext context,
            IAggregateFactory<User, CreateUserSpecification> createUserAggregateFactory)
        {
            _databaseContext = databaseContext;
            _context = context;
            _createUserAggregateFactory = createUserAggregateFactory;
        }

        public async Task Handle(CreateUser message, CancellationToken token)
        {
            var user = await _createUserAggregateFactory
                .Build(new CreateUserSpecification(message.Username, message.Password), token)
                .ConfigureAwait(false);

            await _databaseContext
                .Insert(new[] { new DatabaseModel.User(user.Id, message.Username) }, EnInsertBehavior.Default)
                .CachedExpression("44453009-60BD-4F7A-8042-F397B893E2D6")
                .Invoke(token)
                .ConfigureAwait(false);

            await _context
               .Publish(new UserWasCreated(message.Username), token)
               .ConfigureAwait(false);
        }
    }
}