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
        private readonly OrmSettings _settings;
        private readonly Func<IAdvancedDatabaseTransaction, string, OrmSettings, CancellationToken, Task> _applyModelChange;

        public ModelChange(
            string commandText,
            OrmSettings settings,
            Func<IAdvancedDatabaseTransaction, string, OrmSettings, CancellationToken, Task> applyModelChange)
        {
            _commandText = commandText;
            _settings = settings;
            _applyModelChange = applyModelChange;
        }

        public Task Apply(IDatabaseContext databaseContext, CancellationToken token)
        {
            return _applyModelChange((IAdvancedDatabaseTransaction)databaseContext, _commandText, _settings, token);
        }
    }
}