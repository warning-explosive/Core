namespace SpaceEngineers.Core.Test.WebApplication.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AuthEndpoint.Domain;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using CompositionRoot;
    using DataAccess.Api.Model;
    using DataAccess.Api.Persisting;
    using DataAccess.Orm.Extensions;
    using DataAccess.Orm.Host.Abstractions;
    using DataAccess.Orm.Linq;
    using DataAccess.Orm.Transaction;
    using GenericDomain.Api.Abstractions;
    using GenericDomain.EventSourcing;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(ApplyDeltaMigration))]
    internal class AddSeedDataMigration : IMigration,
                                          ICollectionResolvable<IMigration>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public AddSeedDataMigration(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Name { get; } = nameof(AddSeedDataMigration);

        public bool ApplyEveryTime { get; } = false;

        public Task<IReadOnlyCollection<ICommand>> InvokeCommands(CancellationToken token)
        {
            return _dependencyContainer.InvokeWithinTransaction(true, _dependencyContainer, ExecuteManualMigration, token);
        }

        private static async Task<IReadOnlyCollection<ICommand>> ExecuteManualMigration(
            IAdvancedDatabaseTransaction transaction,
            IDependencyContainer dependencyContainer,
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
                new PermissionWasGranted(new Feature(GenericEndpoint.EventSourcing.Features.EventSourcing)),
                new PermissionWasGranted(new Feature(AuthEndpoint.Contract.Features.Authentication)),
                new PermissionWasGranted(new Feature(Features.WebApiTest))
            };

            var args = domainEvents
                .Select((domainEvent, index) => new DomainEventArgs(aggregateId, domainEvent, index, DateTime.UtcNow))
                .ToArray();

            await dependencyContainer
                .Resolve<IEventStore>()
                .Append(args, token)
                .ConfigureAwait(false);

            var userDatabaseEntity = new AuthEndpoint.DatabaseModel.User(aggregateId, username);

            await transaction
               .Insert(new IDatabaseEntity[] { userDatabaseEntity }, EnInsertBehavior.Default, token)
               .ConfigureAwait(false);

            return transaction.Commands.ToList();
        }
    }
}