namespace SpaceEngineers.Core.AuthorizationEndpoint.Domain
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
    using GenericDomain.Api.Abstractions;

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
               .Read<DatabaseModel.User>()
               .All()
               .Where(user => user.Username == spec.Username)
               .SingleOrDefaultAsync(token)
               .ConfigureAwait(false);

            if (userDatabaseEntity == null)
            {
                throw new NotFoundException($"User '{spec.Username}' doesn't exist");
            }

            var user = await _eventStore
               .Get<User>(userDatabaseEntity.PrimaryKey, DateTime.UtcNow, token)
               .ConfigureAwait(false);

            if (user == null)
            {
                throw new NotFoundException($"User '{spec.Username}' doesn't exist");
            }

            return user;
        }
    }
}