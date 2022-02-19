namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations
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
    using CompositionRoot.Api.Abstractions.Container;
    using CrossCuttingConcerns.Api.Abstractions;
    using Extensions;
    using Model;
    using Orm.Extensions;
    using Orm.Model;
    using Orm.Settings;
    using Reading;

    [Component(EnLifestyle.Singleton)]
    internal class ModelMigrator : IModelMigrator
    {
        private const string AutomaticMigration = "Automatic migration";
        private const string CommandFormat = @"--[{0}]{1}";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IModelProvider _modelProvider;
        private readonly IEnumerable<IManualMigration> _manualMigrations;

        public ModelMigrator(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<OrmSettings> settingsProvider,
            IModelProvider modelProvider,
            IEnumerable<IManualMigration> manualMigrations)
        {
            _dependencyContainer = dependencyContainer;
            _settingsProvider = settingsProvider;
            _modelProvider = modelProvider;
            _manualMigrations = manualMigrations;
        }

        public async Task ExecuteManualMigrations(CancellationToken token)
        {
            var appliedMigrations = await _dependencyContainer
                .InvokeWithinTransaction(ReadAppliedMigrations, token)
                .ConfigureAwait(false);

            await _manualMigrations
                .Where(migration => !appliedMigrations.Contains(migration.Name))
                .Select(migration => migration.ExecuteManualMigration(token))
                .WhenAll()
                .ConfigureAwait(false);
        }

        public async Task ExecuteAutoMigrations(IReadOnlyCollection<IModelChange> modelChanges, CancellationToken token)
        {
            var commandText = await BuildCommands(modelChanges.ToArray(), token)
                .ConfigureAwait(false);

            await _dependencyContainer
                .InvokeWithinTransaction(commandText, ExecuteAutoMigrations, token)
                .ConfigureAwait(false);
        }

        internal static string GetAutomaticMigrationName(int number)
        {
            return string.Join(string.Empty, AutomaticMigration, " ", number);
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

        private async Task ExecuteAutoMigrations(
            IDatabaseTransaction transaction,
            string commandText,
            CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            _ = await transaction
                .UnderlyingDbTransaction
                .InvokeScalar(commandText, settings, token)
                .ConfigureAwait(false);

            var indexes = (await transaction
                    .Read<AppliedMigration, Guid>()
                    .All()
                    .Select(migration => migration.Name)
                    .Where(name => name.Like(AutomaticMigration + "%"))
                    .ToArrayAsync(token)
                    .ConfigureAwait(false)).Select(name =>
                    int.Parse(name.Substring(AutomaticMigration.Length).Trim(), CultureInfo.InvariantCulture))
                .ToArray();

            var nextNumber = indexes.Any()
                ? indexes.Max() + 1
                : 0;

            var appliedMigration = new AppliedMigration(
                Guid.NewGuid(),
                DateTime.Now,
                commandText,
                GetAutomaticMigrationName(nextNumber));

            await transaction
                .Write<AppliedMigration, Guid>()
                .Insert(appliedMigration, token)
                .ConfigureAwait(false);
        }

        private async Task<HashSet<string>> ReadAppliedMigrations(
            IDatabaseTransaction transaction,
            CancellationToken token)
        {
            var isSecondaryMigration = await transaction
                .Read<DatabaseColumnConstraint, Guid>()
                .All()
                .AnyAsync(constraint => constraint.Table == _modelProvider.TableName(typeof(AppliedMigration)), token)
                .ConfigureAwait(false);

            return isSecondaryMigration
                ? await transaction
                    .Read<AppliedMigration, Guid>()
                    .All()
                    .Select(migration => migration.Name)
                    .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, token)
                    .ConfigureAwait(false)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}