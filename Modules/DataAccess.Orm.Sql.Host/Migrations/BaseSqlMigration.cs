namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Connection;
    using Linq;
    using Model;
    using Orm.Host.Abstractions;
    using Orm.Linq;
    using Orm.Transaction;
    using Translation;

    /// <summary>
    /// Base SQL migration
    /// </summary>
    public abstract class BaseSqlMigration : IMigration
    {
        private readonly IDatabaseConnectionProvider _connectionProvider;

        /// <summary> .cctor </summary>
        /// <param name="connectionProvider">IDatabaseConnectionProvider</param>
        protected BaseSqlMigration(IDatabaseConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        /// <summary>
        /// Command text
        /// </summary>
        public abstract string CommandText { get; }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract bool ApplyEveryTime { get; }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ICommand>> InvokeCommands(
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
                .Insert(new[] { appliedMigration }, EnInsertBehavior.Default)
                .Invoke(token)
                .ConfigureAwait(false);

            return commands;
        }
    }
}