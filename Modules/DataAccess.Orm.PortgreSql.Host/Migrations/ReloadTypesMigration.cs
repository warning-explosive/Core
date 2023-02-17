namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Migrations
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using CompositionRoot;
    using Extensions;
    using Linq;
    using Npgsql;
    using Orm.Host.Abstractions;
    using Sql.Translation;
    using Transaction;

    [Component(EnLifestyle.Singleton)]
    [After("SpaceEngineers.Core.DataAccess.Orm.Sql.Host SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Migrations.ApplyDeltaMigration")]
    internal class ReloadTypesMigration : IMigration,
                                          ICollectionResolvable<IMigration>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public ReloadTypesMigration(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Name { get; } = nameof(ReloadTypesMigration);

        public bool ApplyEveryTime { get; } = true;

        public Task<ICommand> InvokeCommand(CancellationToken token)
        {
            return _dependencyContainer.InvokeWithinTransaction(false, ReloadTypes, token);
        }

        private static Task<ICommand> ReloadTypes(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            ((NpgsqlConnection)transaction.DbConnection).ReloadTypes();

            ICommand command = new SqlCommand(
                $"--{nameof(NpgsqlConnection)}.{nameof(NpgsqlConnection.ReloadTypes)}()",
                Array.Empty<SqlCommandParameter>());

            return Task.FromResult(command);
        }
    }
}