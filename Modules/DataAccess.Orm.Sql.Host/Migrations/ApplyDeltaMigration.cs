namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Reading;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using CompositionRoot;
    using Connection;
    using CrossCuttingConcerns.Settings;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using Model;
    using Orm.Extensions;
    using Orm.Host.Migrations;
    using Orm.Settings;
    using Reading;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.DataAccess.Api.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Host.Model;
    using Transaction;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(InitialMigration))]
    internal class ApplyDeltaMigration : IMigration,
                                         ICollectionResolvable<IMigration>
    {
        private const string CommandFormat = @"--[{0}]{1}";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseProvider _databaseProvider;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IDatabaseTypeProvider _databaseTypeProvider;
        private readonly IModelChangesExtractor _modelChangesExtractor;
        private readonly ILogger _logger;

        public ApplyDeltaMigration(
            IDependencyContainer dependencyContainer,
            IDatabaseProvider databaseProvider,
            ISettingsProvider<OrmSettings> settingsProvider,
            IDatabaseTypeProvider databaseTypeProvider,
            IModelChangesExtractor modelChangesExtractor,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _databaseProvider = databaseProvider;
            _settingsProvider = settingsProvider;
            _databaseTypeProvider = databaseTypeProvider;
            _modelChangesExtractor = modelChangesExtractor;
            _logger = logger;
        }

        public virtual string Name { get; } = nameof(ApplyDeltaMigration);

        public virtual bool ApplyEveryTime { get; } = true;

        public async Task<(string name, string commandText)> Migrate(CancellationToken token)
        {
            var databaseEntities = _databaseTypeProvider
               .DatabaseEntities()
               .ToList();

            var modelChanges = await _modelChangesExtractor
               .ExtractChanges(databaseEntities, token)
               .ConfigureAwait(false);

            return await Migrate(Name, modelChanges, token).ConfigureAwait(false);
        }

        private async Task<(string name, string commandText)> Migrate(
            string name,
            IReadOnlyCollection<IModelChange> modelChanges,
            CancellationToken token)
        {
            if (!modelChanges.Any())
            {
                var migrationName = await _dependencyContainer
                   .InvokeWithinTransaction(false, name, GetMigrationName, token)
                   .ConfigureAwait(false);

                return (migrationName, "-- nothing changed");
            }
            else
            {
                var commandText = await BuildCommands(modelChanges.ToArray(), token).ConfigureAwait(false);

                var migrationName = await _dependencyContainer
                   .InvokeWithinTransaction(true, (name, commandText), Migrate, token)
                   .ConfigureAwait(false);

                return (migrationName, commandText);
            }
        }

        private async Task<string> BuildCommands(IModelChange[] modelChanges, CancellationToken token)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < modelChanges.Length; i++)
            {
                var modelChange = modelChanges[i];

                var command = await _dependencyContainer
                    .ResolveGeneric(typeof(IModelChangeMigration<>), modelChange.GetType())
                    .CallMethod(nameof(IModelChangeMigration<IModelChange>.Migrate))
                    .WithArguments(modelChange, token)
                    .Invoke<Task<string>>()
                    .ConfigureAwait(false);

                command = CommandFormat
                    .Format(i, Environment.NewLine + command)
                    .TrimEnd(';');

                sb.Append(command);
                sb.AppendLine(";");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private async Task<string> Migrate(
            IAdvancedDatabaseTransaction transaction,
            (string name, string commandText) state,
            CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var (name, commandText) = state;

            _ = await ExecutionExtensions
               .TryAsync((commandText, settings, _logger), transaction.Execute)
               .Catch<Exception>()
               .Invoke(_databaseProvider.Handle<long>(commandText), token)
               .ConfigureAwait(false);

            var change = new ModelChange(
                commandText,
                settings,
                _logger,
                static (transaction, commandText, ormSettings, logger, token) => transaction.Execute(commandText, ormSettings, logger, token));

            transaction.CollectChange(change);

            return await GetMigrationName(transaction, name, token).ConfigureAwait(false);
        }

        private static async Task<string> GetMigrationName(
            IAdvancedDatabaseTransaction transaction,
            string name,
            CancellationToken token)
        {
            var pattern = name + "%";

            var indexes = (await transaction
                   .Read<AppliedMigration>()
                   .All()
                   .Where(migration => migration.Name.Like(pattern))
                   .Select(migration => migration.Name)
                   .ToArrayAsync(token)
                   .ConfigureAwait(false))
               .Select(migrationName => int.Parse(migrationName.Substring(name.Length).Trim(), CultureInfo.InvariantCulture))
               .ToArray();

            var nextNumber = indexes.Any()
                ? indexes.Max() + 1
                : 0;

            return string.Join(string.Empty, name, " ", nextNumber);
        }
    }
}