namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Dapper;
    using Orm.Connection;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CreateViewDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateView>
    {
        private const string CommandFormat = @"CREATE VIEW ""{0}"".""{1}"" AS {2}";

        private readonly IDatabaseConnectionProvider _databaseConnectionProvider;

        public CreateViewDatabaseModelChangeMigration(IDatabaseConnectionProvider databaseConnectionProvider)
        {
            _databaseConnectionProvider = databaseConnectionProvider;
        }

        public async Task Migrate(CreateView change, CancellationToken token)
        {
            using (var connection = await _databaseConnectionProvider.OpenConnection(token).ConfigureAwait(false))
            {
                await connection
                    .ExecuteAsync(CommandFormat.Format(change.Schema, change.View, change.Query), token)
                    .ConfigureAwait(false);
            }
        }
    }
}