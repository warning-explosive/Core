namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Persisting;
    using Api.Transaction;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Connection;
    using CrossCuttingConcerns.Settings;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using Model;
    using Orm.Extensions;
    using Orm.Host.Migrations;
    using Orm.Settings;
    using Transaction;

    /// <summary>
    /// Base SQL manual migration
    /// </summary>
    public abstract class BaseSqlManualMigration : IManualMigration
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IDatabaseProvider _databaseProvider;
        private readonly ILogger _logger;

        /// <summary> .cctor </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        /// <param name="settingsProvider">Orm setting provider</param>
        /// <param name="databaseProvider">IDatabaseProvider</param>
        /// <param name="logger">ILogger</param>
        protected BaseSqlManualMigration(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<OrmSettings> settingsProvider,
            IDatabaseProvider databaseProvider,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _settingsProvider = settingsProvider;
            _databaseProvider = databaseProvider;
            _logger = logger;
        }

        /// <summary>
        /// Command text
        /// </summary>
        public abstract string CommandText { get; }

        /// <summary>
        /// Migration unique name
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Executes manual migration
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public Task ExecuteManualMigration(CancellationToken token)
        {
            return _dependencyContainer.InvokeWithinTransaction(true, ExecuteManualMigration, token);
        }

        private async Task ExecuteManualMigration(IAdvancedDatabaseTransaction transaction, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var manualMigration = Name;
            var commandText = CommandText;

            _ = await ExecutionExtensions
               .TryAsync((commandText, settings, _logger), transaction.Execute)
               .Catch<Exception>()
               .Invoke(_databaseProvider.Handle<long>(commandText), token)
               .ConfigureAwait(false);

            var change = new ModelChange(
                commandText,
                settings,
                static (transaction, commandText, ormSettings, logger, token) => transaction.Execute(commandText, ormSettings, logger, token));

            transaction.CollectChange(change);

            var appliedMigration = new AppliedMigration(
                Guid.NewGuid(),
                DateTime.Now,
                commandText,
                manualMigration);

            await transaction
                .Write<AppliedMigration, Guid>()
                .Insert(new[] { appliedMigration }, EnInsertBehavior.Default, token)
                .ConfigureAwait(false);
        }
    }
}