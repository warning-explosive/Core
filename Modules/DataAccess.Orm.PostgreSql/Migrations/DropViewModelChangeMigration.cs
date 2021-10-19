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
    internal class DropViewModelChangeMigration : IModelChangeMigration<DropView>
    {
        private const string CommandFormat = @"drop materialized view ""{0}"".""{1}""";

        public Task<string> Migrate(DropView change, CancellationToken token)
        {
            var command = CommandFormat.Format(change.Schema, change.View);

            return Task.FromResult(command);
        }
    }
}