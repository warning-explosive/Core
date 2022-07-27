namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using Microsoft.Extensions.Logging;
    using Settings;

    internal class ModelChange : ITransactionalChange
    {
        private readonly string _commandText;
        private readonly OrmSettings _settings;
        private readonly Func<IAdvancedDatabaseTransaction, string, OrmSettings, ILogger, CancellationToken, Task> _applyModelChange;

        public ModelChange(
            string commandText,
            OrmSettings settings,
            Func<IAdvancedDatabaseTransaction, string, OrmSettings, ILogger, CancellationToken, Task> applyModelChange)
        {
            _commandText = commandText;
            _settings = settings;
            _applyModelChange = applyModelChange;
        }

        public Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            ILogger logger,
            CancellationToken token)
        {
            return _applyModelChange(databaseTransaction, _commandText, _settings, logger, token);
        }

        public void Apply(ITransactionalStore transactionalStore)
        {
        }
    }
}