namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;
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

        public Task<string> BuildCommand(IModelChange change, CancellationToken token)
        {
            return change is CreateDatabase createDatabase
                ? BuildCommand(createDatabase, token)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public async Task<string> BuildCommand(CreateDatabase change, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            throw new InvalidOperationException($"You should create and configure {settings.Database} database manually");
        }
    }
}