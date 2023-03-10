namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;
    using Sql.Host.Model;
    using Sql.Settings;
    using Sql.Translation;

    [Component(EnLifestyle.Singleton)]
    internal class CreateDatabaseModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateDatabase>,
                                                             IResolvable<IModelChangeCommandBuilder<CreateDatabase>>,
                                                             ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private readonly SqlDatabaseSettings _sqlDatabaseSettings;

        public CreateDatabaseModelChangeCommandBuilder(ISettingsProvider<SqlDatabaseSettings> sqlDatabaseSettingsProvider)
        {
            _sqlDatabaseSettings = sqlDatabaseSettingsProvider.Get();
        }

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return change is CreateDatabase createDatabase
                ? BuildCommands(createDatabase)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public IEnumerable<ICommand> BuildCommands(CreateDatabase change)
        {
            throw new InvalidOperationException($"You should create and configure {_sqlDatabaseSettings.Database} database manually");
        }
    }
}