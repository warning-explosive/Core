namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Persisting;
    using Api.Reading;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot;
    using Connection;
    using CrossCuttingConcerns.Logging;
    using Microsoft.Extensions.Logging;
    using Model;
    using Orm.Extensions;
    using Orm.Host.Abstractions;
    using Reading;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class MigrationsExecutor : IMigrationsExecutor,
                                        IResolvable<IMigrationsExecutor>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseConnectionProvider _connectionProvider;
        private readonly IModelProvider _modelProvider;
        private readonly ILogger _logger;

        public MigrationsExecutor(
            IDependencyContainer dependencyContainer,
            IDatabaseConnectionProvider connectionProvider,
            IModelProvider modelProvider,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _connectionProvider = connectionProvider;
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

            var databaseExists = await _connectionProvider
               .DoesDatabaseExist(token)
               .ConfigureAwait(false);

            foreach (var migration in migrations)
            {
                bool apply;

                if (migration.ApplyEveryTime)
                {
                    apply = true;
                }
                else
                {
                    var appliedMigrations = databaseExists
                        ? await _dependencyContainer
                           .InvokeWithinTransaction(false, ReadAppliedMigrations, token)
                           .ConfigureAwait(false)
                        : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    apply = !appliedMigrations.Contains(migration.Name);
                }

                if (!apply)
                {
                    continue;
                }

                var commandText = await migration
                   .Migrate(token)
                   .ConfigureAwait(false);

                await _dependencyContainer
                   .InvokeWithinTransaction(true, (migration, commandText), PersistAppliedMigration, token)
                   .ConfigureAwait(false);

                _logger.Information($"{migration.Name} was applied: {commandText}");
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
            (IMigration migration, string commandText) state,
            CancellationToken token)
        {
            var (migration, commandText) = state;

            var name = migration.ApplyEveryTime
                ? $"{migration.Name} {(await GetMigrationIndex(transaction, migration.Name, token).ConfigureAwait(false)).ToString(CultureInfo.InvariantCulture)}"
                : migration.Name;

            var appliedMigration = new AppliedMigration(
                Guid.NewGuid(),
                DateTime.Now,
                commandText,
                name);

            await transaction
               .Insert(new[] { appliedMigration }, EnInsertBehavior.DoNothing, token)
               .ConfigureAwait(false);
        }

        private static async Task<int> GetMigrationIndex(
            IAdvancedDatabaseTransaction transaction,
            string name,
            CancellationToken token)
        {
            var pattern = name + "%";

            var indexes = (await transaction
                   .All<AppliedMigration>()
                   .Where(migration => migration.Name.Like(pattern))
                   .Select(migration => migration.Name)
                   .ToArrayAsync(token)
                   .ConfigureAwait(false))
               .Select(migrationName => int.Parse(migrationName.Substring(name.Length).Trim(), CultureInfo.InvariantCulture))
               .ToArray();

            return indexes.Any()
                ? indexes.Max() + 1
                : 0;
        }
    }
}