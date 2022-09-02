namespace SpaceEngineers.Core.Test.WebApplication.Migrations
{
    using System;
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
    using GenericDomain.Api.Abstractions;
    using SpaceEngineers.Core.DataAccess.Orm.Host.Migrations;
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

        public Task<(string name, string commandText)> Migrate(CancellationToken token)
        {
            return _dependencyContainer.InvokeWithinTransaction(true, _dependencyContainer, ExecuteManualMigration, token);
        }

        private async Task<(string name, string commandText)> ExecuteManualMigration(
            IAdvancedDatabaseTransaction transaction,
            IDependencyContainer dependencyContainer,
            CancellationToken token)
        {
            var username = "qwerty";
            var password = "12345678";

            var aggregateId = Guid.NewGuid();
            var salt = AuthEndpoint.Domain.Extensions.SecurityExtensions.GenerateSalt();

            var userCreatedDomainEvent = new AuthEndpoint.Domain.Model.UserCreated(
                aggregateId,
                0,
                DateTime.UtcNow,
                new AuthEndpoint.Domain.Model.Username(username),
                salt,
                new AuthEndpoint.Domain.Model.Password(password).GeneratePasswordHash(salt));

            await dependencyContainer
               .Resolve<IEventStore>()
               .Append<AuthEndpoint.Domain.Model.User, AuthEndpoint.Domain.Model.UserCreated>(userCreatedDomainEvent, token)
               .ConfigureAwait(false);

            var userDatabaseEntity = new AuthEndpoint.DatabaseModel.User(aggregateId, username);

            await transaction
               .Write()
               .Insert(new IDatabaseEntity[] { userDatabaseEntity }, EnInsertBehavior.Default, token)
               .ConfigureAwait(false);

            return (Name, $"-- {nameof(AddSeedDataMigration)}");
        }
    }
}