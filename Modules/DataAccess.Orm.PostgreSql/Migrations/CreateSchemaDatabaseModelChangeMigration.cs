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
    internal class CreateSchemaDatabaseModelChangeMigration : IDatabaseModelChangeMigration<CreateSchema>
    {
        private const string CommandFormat = @"CREATE SCHEMA ""{0}""";

        private readonly IDatabaseConnectionProvider _databaseConnectionProvider;

        public CreateSchemaDatabaseModelChangeMigration(IDatabaseConnectionProvider databaseConnectionProvider)
        {
            _databaseConnectionProvider = databaseConnectionProvider;
        }

        public async Task Migrate(CreateSchema change, CancellationToken token)
        {
            using (var connection = await _databaseConnectionProvider.OpenConnection(token).ConfigureAwait(false))
            {
                await connection
                    .ExecuteAsync(CommandFormat.Format(change.Schema), token)
                    .ConfigureAwait(false);
            }
        }
    }
}