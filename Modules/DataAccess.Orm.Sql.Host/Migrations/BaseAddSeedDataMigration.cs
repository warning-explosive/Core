namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics.Primitives;
    using Orm.Transaction;
    using SpaceEngineers.Core.DataAccess.Orm.Host.Abstractions;
    using SpaceEngineers.Core.DataAccess.Orm.Linq;

    /// <summary>
    /// BaseAddSeedDataMigration
    /// </summary>
    public abstract class BaseAddSeedDataMigration : IMigration
    {
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public bool ApplyEveryTime { get; } = false;

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ICommand>> InvokeCommands(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            var buffer = new List<ICommand>();

            using (Disposable.Create(transaction, Do(buffer), Undo(buffer)))
            {
                await AddSeedData(transaction, token).ConfigureAwait(false);

                return transaction.Commands.ToList();
            }

            static Action<IAdvancedDatabaseTransaction> Do(List<ICommand> buffer)
            {
                return transaction =>
                {
                    buffer.AddRange(transaction.Commands);
                    ((List<ICommand>)transaction.Commands).Clear();
                };
            }

            static Action<IAdvancedDatabaseTransaction> Undo(List<ICommand> buffer)
            {
                return transaction =>
                {
                    ((List<ICommand>)transaction.Commands).InsertRange(0, buffer);
                };
            }
        }

        /// <summary>
        /// Adds seed data
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        protected abstract Task AddSeedData(IAdvancedDatabaseTransaction transaction, CancellationToken token);
    }
}