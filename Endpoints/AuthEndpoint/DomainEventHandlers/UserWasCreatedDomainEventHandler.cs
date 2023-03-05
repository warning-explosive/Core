namespace SpaceEngineers.Core.AuthEndpoint.DomainEventHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using DataAccess.Orm.Sql.Linq;
    using DataAccess.Orm.Transaction;
    using Domain;
    using GenericDomain.Api.Abstractions;
    using GenericDomain.EventSourcing;

    [Component(EnLifestyle.Scoped)]
    internal class UserWasCreatedDomainEventHandler : IDomainEventHandler<User, UserWasCreated>,
                                                      IResolvable<IDomainEventHandler<User, UserWasCreated>>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IEventStore _eventStore;

        public UserWasCreatedDomainEventHandler(
            IDatabaseContext databaseContext,
            IEventStore eventStore)
        {
            _databaseContext = databaseContext;
            _eventStore = eventStore;
        }

        public async Task Handle(DomainEventArgs<UserWasCreated> args, CancellationToken token)
        {
            await _eventStore
               .Append(args, token)
               .ConfigureAwait(false);

            var user = new DatabaseModel.User(args.DomainEvent.AggregateId, args.DomainEvent.Username.ToString());

            await _databaseContext
               .Insert(new[] { user }, EnInsertBehavior.Default)
               .Invoke(token)
               .ConfigureAwait(false);
        }
    }
}