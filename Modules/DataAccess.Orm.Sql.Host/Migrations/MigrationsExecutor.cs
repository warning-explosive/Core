namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
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
    using CrossCuttingConcerns.Extensions;
    using Microsoft.Extensions.Logging;
    using Model;
    using Orm.Extensions;
    using Orm.Host.Migrations;
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

                var (name, commandText) = await migration
                   .Migrate(token)
                   .ConfigureAwait(false);

                await _dependencyContainer
                   .InvokeWithinTransaction(true, (name, commandText), PersistAudit, token)
                   .ConfigureAwait(false);

                _logger.Information($"{name} was applied: {commandText}");
            }
        }

        private async Task<HashSet<string>> ReadAppliedMigrations(
            IDatabaseTransaction transaction,
            CancellationToken token)
        {
            var isSecondaryMigration = await transaction
               .Read<DatabaseColumnConstraint>()
               .All()
               .AnyAsync(constraint => constraint.Table == _modelProvider.TableName(typeof(AppliedMigration)), token)
               .ConfigureAwait(false);

            return isSecondaryMigration
                ? await transaction
                   .Read<AppliedMigration>()
                   .All()
                   .Select(migration => migration.Name)
                   .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, token)
                   .ConfigureAwait(false)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private static Task PersistAudit(
            IAdvancedDatabaseTransaction transaction,
            (string name, string commandText) state,
            CancellationToken token)
        {
            var (name, commandText) = state;

            var appliedMigration = new AppliedMigration(
                Guid.NewGuid(),
                DateTime.Now,
                commandText,
                name);

            return transaction
               .Write<AppliedMigration>()
               .Insert(new[] { appliedMigration }, EnInsertBehavior.Default, token);
        }
    }
}