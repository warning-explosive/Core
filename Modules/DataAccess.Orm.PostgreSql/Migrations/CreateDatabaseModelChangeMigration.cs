namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using Orm.Model;
    using Sql.Migrations;
    using Sql.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class CreateDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateDatabase>
    {
        private readonly ISettingsProvider<SqlDatabaseSettings> _settingsProvider;

        public CreateDatabaseModelChangeMigration(ISettingsProvider<SqlDatabaseSettings> settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task<string> Migrate(CreateDatabase change, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            throw new InvalidOperationException($"You should create and configure {settings.Database} database manually");
        }
    }
}