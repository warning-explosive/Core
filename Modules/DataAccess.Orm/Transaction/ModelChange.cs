namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Linq;

    /// <summary>
    /// ModelChange
    /// </summary>
    public class ModelChange : ITransactionalChange
    {
        private readonly ICommand _command;
        private readonly Func<IAdvancedDatabaseTransaction, ICommand, CancellationToken, Task> _applyModelChange;

        /// <summary> .cctor </summary>
        /// <param name="command">Command</param>
        /// <param name="applyModelChange">ApplyModelChange delegate</param>
        public ModelChange(
            ICommand command,
            Func<IAdvancedDatabaseTransaction, ICommand, CancellationToken, Task> applyModelChange)
        {
            _command = command;
            _applyModelChange = applyModelChange;
        }

        /// <inheritdoc />
        public Task Apply(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            return _applyModelChange(transaction, _command, token);
        }

        /// <inheritdoc />
        public void Apply(ITransactionalStore transactionalStore)
        {
        }
    }
}