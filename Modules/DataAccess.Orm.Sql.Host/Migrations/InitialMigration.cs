namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using Api.Model;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot;
    using Connection;
    using CrossCuttingConcerns.Settings;
    using Microsoft.Extensions.Logging;
    using Model;
    using Orm.Host.Model;
    using Orm.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class InitialMigration : ApplyDeltaMigration
    {
        public InitialMigration(
            IDependencyContainer dependencyContainer,
            IDatabaseProvider databaseProvider,
            ISettingsProvider<OrmSettings> settingsProvider,
            IModelChangesExtractor modelChangesExtractor,
            ILogger logger)
            : base(dependencyContainer,
                databaseProvider,
                settingsProvider,
                new InitialMigrationDatabaseTypeProvider(new[] { typeof(AppliedMigration), typeof(SqlView) }),
                modelChangesExtractor,
                logger)
        {
        }

        public override string Name { get; } = nameof(InitialMigration);

        public override bool ApplyEveryTime { get; } = false;

        [UnregisteredComponent]
        private class InitialMigrationDatabaseTypeProvider : IDatabaseTypeProvider,
                                                             IResolvable<IDatabaseTypeProvider>
        {
            private readonly Type[] _databaseEntities;

            public InitialMigrationDatabaseTypeProvider(Type[] databaseEntities)
            {
                _databaseEntities = databaseEntities;
            }

            public IEnumerable<Type> DatabaseEntities()
            {
                return _databaseEntities;
            }
        }
    }
}