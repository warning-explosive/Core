namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Linq;

    /// <summary>
    /// ModelChange
    /// </summary>
    public class ModelChange : ITransactionalChange
    {
        private readonly IReadOnlyCollection<ICommand> _commands;
        private readonly Func<IAdvancedDatabaseTransaction, IReadOnlyCollection<ICommand>, CancellationToken, Task> _applyModelChange;

        /// <summary> .cctor </summary>
        /// <param name="commands">Commands</param>
        /// <param name="applyModelChange">ApplyModelChange delegate</param>
        public ModelChange(
            IReadOnlyCollection<ICommand> commands,
            Func<IAdvancedDatabaseTransaction, IReadOnlyCollection<ICommand>, CancellationToken, Task> applyModelChange)
        {
            _commands = commands;
            _applyModelChange = applyModelChange;
        }

        /// <inheritdoc />
        public Task Apply(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            return _applyModelChange(transaction, _commands, token);
        }

        /// <inheritdoc />
        public void Apply(ITransactionalStore transactionalStore)
        {
        }
    }
}