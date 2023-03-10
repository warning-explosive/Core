namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Connection;
    using Linq;
    using Model;
    using Transaction;
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
                .CachedExpression("D4265ADC-0C92-4B5E-B3E4-B191FEE35AD7")
                .Invoke(token)
                .ConfigureAwait(false);

            return commands;
        }
    }
}