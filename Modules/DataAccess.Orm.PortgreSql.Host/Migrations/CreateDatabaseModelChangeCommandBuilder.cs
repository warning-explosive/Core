namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Linq;
    using Orm.Connection;
    using Sql.Host.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateDatabaseModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateDatabase>,
                                                             IResolvable<IModelChangeCommandBuilder<CreateDatabase>>,
                                                             ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public CreateDatabaseModelChangeCommandBuilder(IDatabaseConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return change is CreateDatabase createDatabase
                ? BuildCommands(createDatabase)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public IEnumerable<ICommand> BuildCommands(CreateDatabase change)
        {
            throw new InvalidOperationException($"You should create and configure {_connectionProvider.Database} database manually");
        }
    }
}