namespace SpaceEngineers.Core.AuthEndpoint.DomainEventHandlers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Exceptions;
    using DataAccess.Api.Reading;
    using DataAccess.Api.Transaction;
    using Domain;
    using GenericDomain.Api.Abstractions;
    using GenericDomain.EventSourcing;

    [Component(EnLifestyle.Scoped)]
    internal class FindUserAggregateFactory : IAggregateFactory<User, FindUserSpecification>,
                                              IResolvable<IAggregateFactory<User, FindUserSpecification>>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IEventStore _eventStore;

        public FindUserAggregateFactory(
            IDatabaseContext databaseContext,
            IEventStore eventStore)
        {
            _databaseContext = databaseContext;
            _eventStore = eventStore;
        }

        public async Task<User> Build(FindUserSpecification spec, CancellationToken token)
        {
            var userDatabaseEntity = await _databaseContext
               .All<DatabaseModel.User>()
               .Where(user => user.Username == spec.Username)
               .SingleOrDefaultAsync(token)
               .ConfigureAwait(false);

            if (userDatabaseEntity == null)
            {
                throw new NotFoundException($"User '{spec.Username}' doesn't exist");
            }

            var user = await _eventStore
               .GetAggregate<User>(userDatabaseEntity.PrimaryKey, DateTime.UtcNow, token)
               .ConfigureAwait(false);

            if (user == null)
            {
                throw new NotFoundException($"User '{spec.Username}' doesn't exist");
            }

            return user;
        }
    }
}