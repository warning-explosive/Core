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
    internal class UserCreatedDomainEventHandler : IDomainEventHandler<User, UserCreated>,
                                                   IResolvable<IDomainEventHandler<User, UserCreated>>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IEventStore _eventStore;

        public UserCreatedDomainEventHandler(
            IDatabaseContext databaseContext,
            IEventStore eventStore)
        {
            _databaseContext = databaseContext;
            _eventStore = eventStore;
        }

        public async Task Handle(UserCreated domainEvent, DomainEventDetails details, CancellationToken token)
        {
            await _eventStore
               .Append<User, UserCreated>(domainEvent, details, token)
               .ConfigureAwait(false);

            await _databaseContext
               .Write<DatabaseModel.User>()
               .Insert(new[] { new DatabaseModel.User(domainEvent.AggregateId, domainEvent.Username.ToString()) }, EnInsertBehavior.Default, token)
               .ConfigureAwait(false);
        }
    }
}