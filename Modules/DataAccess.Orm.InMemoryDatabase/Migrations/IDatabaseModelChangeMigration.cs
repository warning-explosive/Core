namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using Orm.Model;

    /// <summary>
    /// IDatabaseModelChangeMigration
    /// </summary>
    /// <typeparam name="TChange">TChange type-argument</typeparam>
    public interface IDatabaseModelChangeMigration<TChange> : IResolvable
        where TChange : IDatabaseModelChange
    {
        /// <summary>
        /// Migrates
        /// </summary>
        /// <param name="change">Database model change</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Migrate(TChange change, CancellationToken token);
    }
}