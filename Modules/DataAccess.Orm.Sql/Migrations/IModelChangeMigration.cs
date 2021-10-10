namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using Orm.Model;

    /// <summary>
    /// IModelChangeMigration
    /// </summary>
    /// <typeparam name="TChange">TChange type-argument</typeparam>
    public interface IModelChangeMigration<TChange> : IResolvable
        where TChange : IModelChange
    {
        /// <summary>
        /// Migrates
        /// </summary>
        /// <param name="change">Database model change</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<string> Migrate(TChange change, CancellationToken token);
    }
}