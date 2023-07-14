namespace SpaceEngineers.Core.Test.WebApplication.Migrations
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AuthEndpoint.Domain;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using CompositionRoot;
    using DataAccess.Orm.Sql.Linq;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Transaction;
    using GenericDomain.Api.Abstractions;
    using GenericDomain.EventSourcing;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(ApplyDeltaMigration))]
    internal class AddSeedDataMigration : BaseAddSeedDataMigration,
                                          ICollectionResolvable<IMigration>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public AddSeedDataMigration(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public sealed override string Name { get; } = nameof(AddSeedDataMigration);

        protected sealed override async Task AddSeedData(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            var username = "qwerty";
            var password = "12345678";

            var aggregateId = Guid.NewGuid();

            var salt = Password.GenerateSalt();
            var passwordHash = new Password(password).GeneratePasswordHash(salt);

            var domainEvents = new IDomainEvent<User>[]
            {
                new UserWasCreated(aggregateId, new Username(username), salt, passwordHash),
                new PermissionWasGranted(new Feature(AuthEndpoint.Contract.Features.Authentication)),
                new PermissionWasGranted(new Feature(Features.WebApiTest))
            };

            var args = domainEvents
                .Select((domainEvent, index) => new DomainEventArgs(aggregateId, domainEvent, index, DateTime.UtcNow))
                .ToArray();

            await _dependencyContainer
                .Resolve<IEventStore>()
                .Append(args, token)
                .ConfigureAwait(false);

            var userDatabaseEntity = new AuthEndpoint.DatabaseModel.User(aggregateId, username);

            await transaction
                .Insert(new IDatabaseEntity[] { userDatabaseEntity }, EnInsertBehavior.Default)
                .CachedExpression("4271E906-F346-46FB-877C-675818B148E5")
                .Invoke(token)
                .ConfigureAwait(false);
        }
    }
}