namespace SpaceEngineers.Core.Test.WebApplication.Migrations
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using CompositionRoot;
    using DataAccess.Api.Model;
    using DataAccess.Api.Persisting;
    using DataAccess.Api.Transaction;
    using DataAccess.Orm.Extensions;
    using DataAccess.Orm.Host.Abstractions;
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

        public Task<string> Migrate(CancellationToken token)
        {
            return _dependencyContainer.InvokeWithinTransaction(true, _dependencyContainer, ExecuteManualMigration, token);
        }

        private async Task<string> ExecuteManualMigration(
            IAdvancedDatabaseTransaction transaction,
            IDependencyContainer dependencyContainer,
            CancellationToken token)
        {
            var sb = new StringBuilder();

            var username = "qwerty";
            var password = "12345678";

            var aggregateId = Guid.NewGuid();

            var salt = AuthEndpoint.Domain.Model.Password.GenerateSalt();
            var passwordHash = new AuthEndpoint.Domain.Model.Password(password).GeneratePasswordHash(salt);

            var domainEvents = new IDomainEvent<AuthEndpoint.Domain.Model.User>[]
            {
                new AuthEndpoint.Domain.Model.UserWasCreated(aggregateId, new AuthEndpoint.Domain.Model.Username(username), salt, passwordHash),
                new AuthEndpoint.Domain.Model.PermissionWasGranted(new AuthEndpoint.Domain.Model.Feature("Authentication")),
                new AuthEndpoint.Domain.Model.PermissionWasGranted(new AuthEndpoint.Domain.Model.Feature("WebApiTest"))
            };

            var eventStore = dependencyContainer.Resolve<IEventStore>();

            for (var index = 0; index < domainEvents.Length; index++)
            {
                var details = new DomainEventDetails(aggregateId, index, DateTime.UtcNow);

                await eventStore
                    .CallMethod(nameof(eventStore.Append))
                    .WithTypeArguments(typeof(AuthEndpoint.Domain.Model.User), domainEvents[index].GetType())
                    .WithArguments(domainEvents[index], details, token)
                    .Invoke<Task>()
                    .ConfigureAwait(false);
            }

            sb.AppendLine($"--{nameof(AuthEndpoint.Domain.Model.UserWasCreated)}");
            sb.AppendLine(transaction.LastCommand ?? throw new InvalidOperationException($"Unable to find persis command for {nameof(AuthEndpoint.Domain.Model.UserWasCreated)} domain event"));

            var userDatabaseEntity = new AuthEndpoint.DatabaseModel.User(aggregateId, username);

            await transaction
               .Write()
               .Insert(new IDatabaseEntity[] { userDatabaseEntity }, EnInsertBehavior.Default, token)
               .ConfigureAwait(false);

            sb.AppendLine($"--{nameof(AuthEndpoint.DatabaseModel.User)}");
            sb.AppendLine(transaction.LastCommand ?? throw new InvalidOperationException($"Unable to find persis command for {nameof(AuthEndpoint.DatabaseModel.User)} database entity"));

            return sb.ToString();
        }
    }
}