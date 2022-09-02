namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using Microsoft.Extensions.Logging;
    using Settings;

    /// <summary>
    /// ModelChange
    /// </summary>
    public class ModelChange : ITransactionalChange
    {
        private readonly string _commandText;
        private readonly OrmSettings _settings;
        private readonly ILogger _logger;
        private readonly Func<IAdvancedDatabaseTransaction, string, OrmSettings, ILogger, CancellationToken, Task> _applyModelChange;

        /// <summary> .cctor </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="settings">OrmSettings</param>
        /// <param name="logger">ILogger</param>
        /// <param name="applyModelChange">ApplyModelChange delegate</param>
        public ModelChange(
            string commandText,
            OrmSettings settings,
            ILogger logger,
            Func<IAdvancedDatabaseTransaction, string, OrmSettings, ILogger, CancellationToken, Task> applyModelChange)
        {
            _commandText = commandText;
            _settings = settings;
            _logger = logger;
            _applyModelChange = applyModelChange;
        }

        /// <inheritdoc />
        public Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            CancellationToken token)
        {
            return _applyModelChange(databaseTransaction, _commandText, _settings, _logger, token);
        }

        /// <inheritdoc />
        public void Apply(ITransactionalStore transactionalStore)
        {
        }
    }
}