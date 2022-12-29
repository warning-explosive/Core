namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Sql.Host.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateSchemaModelChangeCommandBuilder : IModelChangeCommandBuilder<CreateSchema>,
                                                           IResolvable<IModelChangeCommandBuilder<CreateSchema>>,
                                                           ICollectionResolvable<IModelChangeCommandBuilder>
    {
        private const string CommandFormat = @"create schema ""{0}""";

        public Task<string> BuildCommand(IModelChange change, CancellationToken token)
        {
            return change is CreateSchema createSchema
                ? BuildCommand(createSchema, token)
                : throw new NotSupportedException($"Unsupported model change type {change.GetType()}");
        }

        public Task<string> BuildCommand(CreateSchema change, CancellationToken token)
        {
            var commandText = CommandFormat.Format(change.Schema);

            return Task.FromResult(commandText);
        }
    }
}