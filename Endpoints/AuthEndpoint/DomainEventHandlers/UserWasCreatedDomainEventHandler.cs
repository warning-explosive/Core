namespace SpaceEngineers.Core.AuthEndpoint.DomainEventHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using DataAccess.Api.Persisting;
    using DataAccess.Api.Transaction;
    using Domain.Model;
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

        public async Task Handle(UserWasCreated domainEvent, DomainEventDetails details, CancellationToken token)
        {
            await _eventStore
               .Append<User, UserWasCreated>(domainEvent, details, token)
               .ConfigureAwait(false);

            await _databaseContext
               .Insert(new[] { new DatabaseModel.User(domainEvent.AggregateId, domainEvent.Username.ToString()) }, EnInsertBehavior.Default, token)
               .ConfigureAwait(false);
        }
    }
}