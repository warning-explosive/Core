namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Persisting;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot;
    using CrossCuttingConcerns.Logging;
    using Execution;
    using Microsoft.Extensions.Logging;
    using Model;
    using Orm.Host.Abstractions;
    using Orm.Linq;
    using Sql.Model;
    using Transaction;
    using Translation;

    [Component(EnLifestyle.Singleton)]
    internal class MigrationsExecutor : IMigrationsExecutor,
                                        IResolvable<IMigrationsExecutor>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IModelProvider _modelProvider;
        private readonly ILogger _logger;

        public MigrationsExecutor(
            IDependencyContainer dependencyContainer,
            IModelProvider modelProvider,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _modelProvider = modelProvider;
            _logger = logger;
        }

        public async Task Migrate(
            IReadOnlyCollection<IMigration> migrations,
            CancellationToken token)
        {
            if (!migrations.Any())
            {
                return;
            }

            await _dependencyContainer
                .InvokeWithinTransaction(true, migrations, ApplyMigrations, token)
                .ConfigureAwait(false);
        }

        private async Task ApplyMigrations(
            IAdvancedDatabaseTransaction transaction,
            IReadOnlyCollection<IMigration> migrations,
            CancellationToken token)
        {
            foreach (var migration in migrations)
            {
                bool apply;

                if (migration.ApplyEveryTime)
                {
                    apply = true;
                }
                else
                {
                    var appliedMigrations = await ReadAppliedMigrations(transaction, token).ConfigureAwait(false);

                    apply = !appliedMigrations.Contains(migration.Name);
                }

                if (!apply)
                {
                    continue;
                }

                var commands = await migration.InvokeCommands(transaction, token).ConfigureAwait(false);

                await PersistAppliedMigration(transaction, migration, commands, token).ConfigureAwait(false);

                _logger.Information($"{migration.Name} was applied");
            }
        }

        private async Task<HashSet<string>> ReadAppliedMigrations(
            IDatabaseTransaction transaction,
            CancellationToken token)
        {
            var isSecondaryMigration = await transaction
                .All<DatabaseColumnConstraint>()
                .AnyAsync(constraint => constraint.Table == _modelProvider.TableName(typeof(AppliedMigration)), token)
                .ConfigureAwait(false);

            return isSecondaryMigration
                ? await transaction
                    .All<AppliedMigration>()
                    .Select(migration => migration.Name)
                    .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, token)
                    .ConfigureAwait(false)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private static async Task PersistAppliedMigration(
            IAdvancedDatabaseTransaction transaction,
            IMigration migration,
            IReadOnlyCollection<ICommand> commands,
            CancellationToken token)
        {
            var name = migration.ApplyEveryTime
                ? $"{migration.Name} {(await GetMigrationIndex(transaction, migration.Name, token).ConfigureAwait(false)).ToString(CultureInfo.InvariantCulture)}"
                : migration.Name;

            var commandText = commands
                .Cast<SqlCommand>()
                .Aggregate((acc, next) => acc.Merge(next, ";" + Environment.NewLine))
                .ToString();

            var appliedMigration = new AppliedMigration(
                Guid.NewGuid(),
                DateTime.Now,
                commandText,
                name);

            await transaction
                .Insert(new[] { appliedMigration }, EnInsertBehavior.Default, token)
                .ConfigureAwait(false);

            static async Task<int> GetMigrationIndex(
                IAdvancedDatabaseTransaction transaction,
                string name,
                CancellationToken token)
            {
                var pattern = name + "%";

                var indexes = (await transaction
                        .All<AppliedMigration>()
                        .Where(migration => migration.Name.Like(pattern))
                        .Select(migration => migration.Name)
                        .ToListAsync(token)
                        .ConfigureAwait(false))
                    .Select(migrationName => int.Parse(migrationName.Substring(name.Length).Trim(), CultureInfo.InvariantCulture))
                    .ToArray();

                return indexes.Any()
                    ? indexes.Max() + 1
                    : 0;
            }
        }
    }
}