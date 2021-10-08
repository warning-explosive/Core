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
    internal class CreateViewDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateView>
    {
        private const string CommandFormat = @"create materialized view ""{0}"".""{1}"" as {2}";

        public Task<string> Migrate(CreateView change, CancellationToken token)
        {
            var command = CommandFormat.Format(change.Schema, change.View, change.Query);

            return Task.FromResult(command);
        }
    }
}