namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

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