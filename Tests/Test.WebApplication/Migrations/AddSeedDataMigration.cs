namespace SpaceEngineers.Core.Test.WebApplication.Migrations
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
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
            var salt = AuthEndpoint.Domain.Model.Password.GenerateSalt();

            var aggregateId = Guid.NewGuid();

            var userCreatedDomainEvent = new AuthEndpoint.Domain.Model.UserCreated(
                aggregateId,
                new AuthEndpoint.Domain.Model.Username(username),
                salt,
                new AuthEndpoint.Domain.Model.Password(password).GeneratePasswordHash(salt));

            var details = new DomainEventDetails(aggregateId, 0, DateTime.UtcNow);

            await dependencyContainer
               .Resolve<IEventStore>()
               .Append<AuthEndpoint.Domain.Model.User, AuthEndpoint.Domain.Model.UserCreated>(userCreatedDomainEvent, details, token)
               .ConfigureAwait(false);

            sb.AppendLine($"--{nameof(AuthEndpoint.Domain.Model.UserCreated)}");
            sb.AppendLine(transaction.LastCommand ?? throw new InvalidOperationException($"Unable to find persis command for {nameof(AuthEndpoint.Domain.Model.UserCreated)} domain event"));

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