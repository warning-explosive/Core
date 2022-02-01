namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using CompositionRoot.Api.Abstractions.Container;
    using CrossCuttingConcerns.Api.Abstractions;
    using Extensions;
    using Model;
    using Orm.Extensions;
    using Orm.Model;
    using Orm.Settings;

    /// <summary>
    /// Base SQL manual migration
    /// </summary>
    public abstract class BaseSqlManualMigration : IManualMigration,
                                                   ICollectionResolvable<IManualMigration>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;

        /// <summary> .cctor </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        /// <param name="settingsProvider">Orm setting provider</param>
        protected BaseSqlManualMigration(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<OrmSettings> settingsProvider)
        {
            _dependencyContainer = dependencyContainer;
            _settingsProvider = settingsProvider;
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
            return _dependencyContainer.InvokeWithinTransaction(Producer, token);
        }

        private async Task Producer(IDatabaseTransaction transaction, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var manualMigration = Name;
            var commandText = CommandText;

            _ = await transaction
                .UnderlyingDbTransaction
                .Invoke(commandText, settings, token)
                .ConfigureAwait(false);

            var appliedMigration = new AppliedMigration(
                Guid.NewGuid(),
                DateTime.Now,
                commandText,
                manualMigration);

            await transaction
                .Write<AppliedMigration, Guid>()
                .Insert(appliedMigration, token)
                .ConfigureAwait(false);
        }
    }
}