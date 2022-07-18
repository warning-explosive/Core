namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using Settings;

    internal class ModelChange : ITransactionalChange
    {
        private readonly string _commandText;
        private readonly long _affectedRowsCount;
        private readonly OrmSettings _settings;
        private readonly Func<IAdvancedDatabaseTransaction, string, OrmSettings, CancellationToken, Task> _applyModelChange;

        public ModelChange(
            string commandText,
            long affectedRowsCount,
            OrmSettings settings,
            Func<IAdvancedDatabaseTransaction, string, OrmSettings, CancellationToken, Task> applyModelChange)
        {
            _commandText = commandText;
            _affectedRowsCount = affectedRowsCount;
            _settings = settings;
            _applyModelChange = applyModelChange;
        }

        public Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            CancellationToken token)
        {
            return _applyModelChange(databaseTransaction, _commandText, _settings, token);
        }
    }
}