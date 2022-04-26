namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Persisting;
    using Api.Reading;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Connection;
    using CrossCuttingConcerns.Settings;
    using Extensions;
    using Model;
    using Orm.Extensions;
    using Orm.Host.Migrations;
    using Orm.Host.Model;
    using Orm.Settings;
    using Reading;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ModelMigrationsExecutor : IModelMigrationsExecutor,
                                             IResolvable<IModelMigrationsExecutor>
    {
        private const string AutomaticMigration = "Automatic migration";
        private const string CommandFormat = @"--[{0}]{1}";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IDatabaseConnectionProvider _connectionProvider;
        private readonly IModelProvider _modelProvider;

        public ModelMigrationsExecutor(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<OrmSettings> settingsProvider,
            IDatabaseConnectionProvider connectionProvider,
            IModelProvider modelProvider)
        {
            _dependencyContainer = dependencyContainer;
            _settingsProvider = settingsProvider;
            _connectionProvider = connectionProvider;
            _modelProvider = modelProvider;
        }

        public async Task ExecuteManualMigrations(
            IReadOnlyCollection<IManualMigration> manualMigrations,
            CancellationToken token)
        {
            if (!manualMigrations.Any())
            {
                return;
            }

            var databaseExists = await _connectionProvider
               .DoesDatabaseExist(token)
               .ConfigureAwait(false);

            foreach (var migration in manualMigrations)
            {
                var appliedMigrations = databaseExists
                    ? await _dependencyContainer
                       .InvokeWithinTransaction(false, ReadAppliedMigrations, token)
                       .ConfigureAwait(false)
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (!appliedMigrations.Contains(migration.Name))
                {
                    await migration
                       .ExecuteManualMigration(token)
                       .ConfigureAwait(false);
                }
            }
        }

        public async Task ExecuteAutoMigrations(IReadOnlyCollection<IModelChange> modelChanges, CancellationToken token)
        {
            if (!modelChanges.Any())
            {
                return;
            }

            var commandText = await BuildCommands(modelChanges.ToArray(), token)
                .ConfigureAwait(false);

            await _dependencyContainer
                .InvokeWithinTransaction(true, commandText, ExecuteAutoMigrations, token)
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

            try
            {
                _ = await transaction
                    .UnderlyingDbTransaction
                    .InvokeScalar(commandText, settings, token)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(commandText, exception);
            }

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
                .Insert(new[] { appliedMigration }, EnInsertBehavior.Default, token)
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