namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Model;
    using Sql.Migrations;

    [Component(EnLifestyle.Singleton)]
    internal class CreateIndexDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateIndex>
    {
        private const string UniqieCommandFormat = @"create unique index ""{2}"" on ""{0}"".""{1}"" ({3})";
        private const string CommandFormat = @"create index ""{2}"" on ""{0}"".""{1}"" ({3})";

        public Task<string> Migrate(CreateIndex change, CancellationToken token)
        {
            var columns = change
                .Columns
                .Select(column => $@"""{column}""")
                .ToString(", ");

            var commandTextFormat = change.Unique
                ? UniqieCommandFormat
                : CommandFormat;

            var command = commandTextFormat.Format(change.Schema, change.Table, change.Index, columns);

            return Task.FromResult(command);
        }
    }
}