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

        public FindUserAggregateFactory(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<User> Build(FindUserSpecification spec, CancellationToken token)
        {
            var userDatabaseEntity = await _databaseContext
                .Read<DatabaseModel.User, Guid>()
                .All()
                .Where(user => user.Username == spec.Username)
                .SingleOrDefaultAsync(token)
                .ConfigureAwait(false);

            return userDatabaseEntity == null
                ? throw new NotFoundException($"User '{spec.Username}' doesn't exist")
                : new User(userDatabaseEntity);
        }
    }
}