namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IModelChangeCommandBuilder
    /// </summary>
    /// <typeparam name="TChange">TChange type-argument</typeparam>
    public interface IModelChangeCommandBuilder<TChange>
        where TChange : IModelChange
    {
        /// <summary>
        /// Migrates
        /// </summary>
        /// <param name="change">Database model change</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<string> BuildCommand(TChange change, CancellationToken token);
    }
}