namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Connection;
    using Model;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class InitialMigration : ApplyDeltaMigration
    {
        public InitialMigration(
            IModelChangesExtractor modelChangesExtractor,
            IModelChangeCommandBuilderComposite commandBuilder,
            IDatabaseConnectionProvider connectionProvider)
            : base(
                new InitialMigrationDatabaseTypeProvider(new[] { typeof(AppliedMigration), typeof(SqlView), typeof(FunctionView) }),
                modelChangesExtractor,
                commandBuilder,
                connectionProvider)
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