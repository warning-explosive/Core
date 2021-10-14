namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using CrossCuttingConcerns.Api.Abstractions;
    using Dapper;
    using Orm.Model;
    using Orm.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class ModelMigrator : IModelMigrator
    {
        private const string CommandFormat = @"--[{0}]{1}";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;

        public ModelMigrator(IDependencyContainer dependencyContainer, ISettingsProvider<OrmSettings> settingsProvider)
        {
            _dependencyContainer = dependencyContainer;
            _settingsProvider = settingsProvider;
        }

        public async Task Migrate(IReadOnlyCollection<IModelChange> modelChanges, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var commandText = await BuildCommands(modelChanges.ToArray(), token).ConfigureAwait(false);

            var command = new CommandDefinition(
                commandText,
                null,
                null,
                settings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            await using (_dependencyContainer.OpenScopeAsync())
            {
                var transaction = _dependencyContainer.Resolve<IAdvancedDatabaseTransaction>();

                await using (await transaction.Open(true, token).ConfigureAwait(false))
                {
                    await transaction
                        .UnderlyingDbTransaction
                        .Connection
                        .ExecuteAsync(command)
                        .ConfigureAwait(false);
                }
            }
        }

        private async Task<string> BuildCommands(IModelChange[] modelChanges, CancellationToken token)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < modelChanges.Length; i++)
            {
                var modelChange = modelChanges[i];

                var command = await _dependencyContainer
                    .ResolveGeneric(typeof(IModelChangeMigration<>), modelChange.GetType())
                    .CallMethod(nameof(IModelChangeMigration<IModelChange>.Migrate))
                    .WithArguments(modelChange, token)
                    .Invoke<Task<string>>()
                    .ConfigureAwait(false);

                command = CommandFormat
                    .Format(i, Environment.NewLine + command)
                    .TrimEnd(';');

                sb.Append(command);
                sb.AppendLine(";");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}