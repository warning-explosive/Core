namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Host.Model;
    using Sql.Host.Migrations;

    [Component(EnLifestyle.Singleton)]
    internal class CreateSchemaModelChangeMigration : IModelChangeMigration<CreateSchema>,
                                                      IResolvable<IModelChangeMigration<CreateSchema>>
    {
        private const string CommandFormat = @"create schema ""{0}""";

        public Task<string> Migrate(CreateSchema change, CancellationToken token)
        {
            var commandText = CommandFormat.Format(change.Schema);

            return Task.FromResult(commandText);
        }
    }
}