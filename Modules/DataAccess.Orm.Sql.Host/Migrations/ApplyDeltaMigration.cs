namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using Connection;
    using Model;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using Sql.Model;
    using Transaction;
    using Translation;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(InitialMigration))]
    internal class ApplyDeltaMigration : IMigration,
                                         ICollectionResolvable<IMigration>
    {
        private readonly IDatabaseTypeProvider _databaseTypeProvider;
        private readonly IModelChangesExtractor _modelChangesExtractor;
        private readonly IModelChangeCommandBuilderComposite _commandBuilder;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public ApplyDeltaMigration(
            IDatabaseTypeProvider databaseTypeProvider,
            IModelChangesExtractor modelChangesExtractor,
            IModelChangeCommandBuilderComposite commandBuilder,
            IDatabaseConnectionProvider connectionProvider)
        {
            _databaseTypeProvider = databaseTypeProvider;
            _modelChangesExtractor = modelChangesExtractor;
            _commandBuilder = commandBuilder;
            _connectionProvider = connectionProvider;
        }

        public virtual string Name { get; } = nameof(ApplyDeltaMigration);

        public virtual bool ApplyEveryTime { get; } = true;

        public async Task<IReadOnlyCollection<ICommand>> InvokeCommands(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            var databaseEntities = _databaseTypeProvider
               .DatabaseEntities()
               .ToList();

            var modelChanges = await _modelChangesExtractor
               .ExtractChanges(transaction, databaseEntities, token)
               .ConfigureAwait(false);

            if (!modelChanges.Any())
            {
                return new[] { new SqlCommand("--nothing was changed", Array.Empty<SqlCommandParameter>()) };
            }

            var commands = modelChanges
                .SelectMany(modelChange => _commandBuilder.BuildCommands(modelChange))
                .Select(command =>
                {
                    if (command is not SqlCommand sqlCommand)
                    {
                        throw new NotSupportedException($"Unsupported command type {command.GetType()}");
                    }

                    return sqlCommand;
                })
                .ToList();

            _ = await _connectionProvider
                .Execute(transaction, commands, token)
                .ConfigureAwait(false);

            var change = new ModelChange(commands, _connectionProvider.Execute);

            transaction.CollectChange(change);

            return commands;
        }
    }
}