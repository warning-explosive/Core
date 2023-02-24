namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;
    using Linq;
    using Sql.Host.Model;
    using Sql.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class CreateDatabaseModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateDatabase>,
                                                             IResolvable<IModelChangeCommandBuilder<CreateDatabase>>,
                                                             ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private readonly ISettingsProvider<SqlDatabaseSettings> _settingsProvider;

        public CreateDatabaseModelChangeCommandBuilder(ISettingsProvider<SqlDatabaseSettings> settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return change is CreateDatabase createDatabase
                ? BuildCommands(createDatabase)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public IEnumerable<ICommand> BuildCommands(CreateDatabase change)
        {
            var settings = _settingsProvider.Get(CancellationToken.None).Result;

            throw new InvalidOperationException($"You should create and configure {settings.Database} database manually");
        }
    }
}