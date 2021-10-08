namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Migrations
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelMigrator : IDatabaseModelMigrator
    {
        private readonly IDependencyContainer _dependencyContainer;

        public DatabaseModelMigrator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task Migrate(IReadOnlyCollection<IDatabaseModelChange> modelChanges, CancellationToken token)
        {
            foreach (var change in modelChanges)
            {
                await using (_dependencyContainer.OpenScopeAsync())
                {
                    await _dependencyContainer
                        .ResolveGeneric(typeof(IDatabaseModelChangeMigration<>), change.GetType())
                        .CallMethod(nameof(IDatabaseModelChangeMigration<IDatabaseModelChange>.Migrate))
                        .WithArguments(change, token)
                        .Invoke<Task>()
                        .ConfigureAwait(false);
                }
            }
        }
    }
}