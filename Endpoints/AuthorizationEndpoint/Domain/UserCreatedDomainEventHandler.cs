namespace SpaceEngineers.Core.AuthorizationEndpoint.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using DataAccess.Api.Persisting;
    using DataAccess.Api.Transaction;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class UserCreatedDomainEventHandler : IDomainEventHandler<UserCreated>,
                                                   IResolvable<IDomainEventHandler<UserCreated>>
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

        public async Task Handle(UserCreated domainEvent, CancellationToken token)
        {
            var primaryKey = Guid.NewGuid();

            // TODO: #172 - store domain events automatically
            await _eventStore
               .Append<User, UserCreated>(primaryKey, domainEvent)
               .ConfigureAwait(false);

            await _databaseContext
               .Write<DatabaseModel.User, Guid>()
               .Insert(new[] { new DatabaseModel.User(primaryKey, domainEvent.Username) }, EnInsertBehavior.Default, token)
               .ConfigureAwait(false);
        }
    }
}