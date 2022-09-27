namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Persisting;
    using Api.Transaction;
    using Basics;
    using CompositionRoot;
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
    /// Base SQL migration
    /// </summary>
    public abstract class BaseSqlMigration : IMigration
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
        protected BaseSqlMigration(
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
        /// Migration's unique name
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// If returns true migration will be executed during startup process despite the fact that it could be applied earlier
        /// </summary>
        public abstract bool ApplyEveryTime { get; }

        /// <summary>
        /// Executes migration
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public Task<string> Migrate(CancellationToken token)
        {
            return _dependencyContainer.InvokeWithinTransaction(true, ExecuteManualMigration, token);
        }

        private async Task<string> ExecuteManualMigration(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            _ = await ExecutionExtensions
               .TryAsync((CommandText, settings, _logger), transaction.Execute)
               .Catch<Exception>()
               .Invoke(_databaseProvider.Handle<long>(CommandText), token)
               .ConfigureAwait(false);

            var change = new ModelChange(
                CommandText,
                settings,
                _logger,
                static (transaction, commandText, ormSettings, logger, token) => transaction.Execute(commandText, ormSettings, logger, token));

            transaction.CollectChange(change);

            var appliedMigration = new AppliedMigration(
                Guid.NewGuid(),
                DateTime.Now,
                CommandText,
                Name);

            await transaction
                .Write<AppliedMigration>()
                .Insert(new[] { appliedMigration }, EnInsertBehavior.Default, token)
                .ConfigureAwait(false);

            return CommandText;
        }
    }
}