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
    using CompositionRoot;
    using Connection;
    using Extensions;
    using Model;
    using Orm.Host.Abstractions;
    using Orm.Linq;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.DataAccess.Api.Model;
    using Transaction;
    using Translation;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(InitialMigration))]
    internal class ApplyDeltaMigration : IMigration,
                                         ICollectionResolvable<IMigration>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseTypeProvider _databaseTypeProvider;
        private readonly IModelChangesExtractor _modelChangesExtractor;
        private readonly IModelChangeCommandBuilderComposite _commandBuilder;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public ApplyDeltaMigration(
            IDependencyContainer dependencyContainer,
            IDatabaseTypeProvider databaseTypeProvider,
            IModelChangesExtractor modelChangesExtractor,
            IModelChangeCommandBuilderComposite commandBuilder,
            IDatabaseConnectionProvider connectionProvider)
        {
            _dependencyContainer = dependencyContainer;
            _databaseTypeProvider = databaseTypeProvider;
            _modelChangesExtractor = modelChangesExtractor;
            _commandBuilder = commandBuilder;
            _connectionProvider = connectionProvider;
        }

        public virtual string Name { get; } = nameof(ApplyDeltaMigration);

        public virtual bool ApplyEveryTime { get; } = true;

        public async Task<ICommand> Migrate(CancellationToken token)
        {
            var databaseEntities = _databaseTypeProvider
               .DatabaseEntities()
               .ToList();

            var modelChanges = await _modelChangesExtractor
               .ExtractChanges(databaseEntities, token)
               .ConfigureAwait(false);

            return await Migrate(modelChanges, token).ConfigureAwait(false);
        }

        private async Task<ICommand> Migrate(
            IReadOnlyCollection<IModelChange> modelChanges,
            CancellationToken token)
        {
            if (!modelChanges.Any())
            {
                return new SqlCommand("--nothing was changed", Array.Empty<SqlCommandParameter>());
            }

            var command = BuildCommands(modelChanges.ToArray());

            await _dependencyContainer
               .InvokeWithinTransaction(true, command, Migrate, token)
               .ConfigureAwait(false);

            return command;
        }

        private ICommand BuildCommands(IModelChange[] modelChanges)
        {
            return modelChanges
                .SelectMany(modelChange => _commandBuilder.BuildCommands(modelChange))
                .Select(command =>
                {
                    if (command is not SqlCommand sqlCommand)
                    {
                        throw new NotSupportedException($"Unsupported command type {command.GetType()}");
                    }

                    return sqlCommand;
                })
                .Aggregate((acc, next) => acc.Merge(next, ";" + Environment.NewLine + Environment.NewLine));
        }

        private async Task Migrate(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token)
        {
            _ = await _connectionProvider
                .Execute(transaction, command, token)
                .ConfigureAwait(false);

            var change = new ModelChange(command, _connectionProvider.Execute);

            transaction.CollectChange(change);
        }
    }
}