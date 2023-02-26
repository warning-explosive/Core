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
    using DataAccess.Api.Transaction;
    using DataAccess.Orm.Linq;
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
               .CachedExpression("24698B87-8B82-4EC4-B605-3C4711630979")
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