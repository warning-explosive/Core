namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Model;
    using Sql.Migrations;

    [Component(EnLifestyle.Singleton)]
    internal class CreateSchemaModelChangeMigration : IModelChangeMigration<CreateSchema>
    {
        private const string CommandFormat = @"create schema ""{0}""";

        public Task<string> Migrate(CreateSchema change, CancellationToken token)
        {
            var command = CommandFormat.Format(change.Schema);

            return Task.FromResult(command);
        }
    }
}