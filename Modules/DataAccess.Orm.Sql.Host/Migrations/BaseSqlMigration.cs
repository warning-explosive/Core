namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Persisting;
    using CompositionRoot;
    using Connection;
    using Extensions;
    using Model;
    using Orm.Host.Abstractions;
    using Orm.Linq;
    using Transaction;
    using Translation;

    /// <summary>
    /// Base SQL migration
    /// </summary>
    public abstract class BaseSqlMigration : IMigration
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        /// <summary> .cctor </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        /// <param name="connectionProvider">IDatabaseConnectionProvider</param>
        protected BaseSqlMigration(
            IDependencyContainer dependencyContainer,
            IDatabaseConnectionProvider connectionProvider)
        {
            _dependencyContainer = dependencyContainer;
            _connectionProvider = connectionProvider;
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
        public Task<IReadOnlyCollection<ICommand>> InvokeCommands(CancellationToken token)
        {
            return _dependencyContainer.InvokeWithinTransaction(true, ExecuteManualMigration, token);
        }

        private async Task<IReadOnlyCollection<ICommand>> ExecuteManualMigration(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            var command = new SqlCommand(CommandText, Array.Empty<SqlCommandParameter>());

            _ = await _connectionProvider
                .Execute(transaction, command, token)
                .ConfigureAwait(false);

            var commands = new[] { command };

            var change = new ModelChange(commands, _connectionProvider.Execute);

            transaction.CollectChange(change);

            var appliedMigration = new AppliedMigration(
                Guid.NewGuid(),
                DateTime.Now,
                command.CommandText,
                Name);

            await transaction
                .Insert(new[] { appliedMigration }, EnInsertBehavior.Default, token)
                .ConfigureAwait(false);

            return commands;
        }
    }
}