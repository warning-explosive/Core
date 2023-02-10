namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Linq;
    using Sql.Host.Model;
    using Sql.Translation;

    [Component(EnLifestyle.Singleton)]
    internal class CreateSchemaModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateSchema>,
                                                           IResolvable<IModelChangeCommandBuilder<CreateSchema>>,
                                                           ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string CommandFormat = @"create schema ""{0}""";

        public IEnumerable<ICommand> BuildCommands(IModelChange change)
        {
            return change is CreateSchema createSchema
                ? BuildCommands(createSchema)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public IEnumerable<ICommand> BuildCommands(CreateSchema change)
        {
            yield return new SqlCommand(CommandFormat.Format(change.Schema), Array.Empty<SqlCommandParameter>());
        }
    }
}