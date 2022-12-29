namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IModelChangeCommandBuilderComposite
    /// </summary>
    public interface IModelChangeCommandBuilderComposite : IModelChangeCommandBuilder
    {
    }

    /// <summary>
    /// IModelChangeCommandBuilder
    /// </summary>
    public interface IModelChangeCommandBuilder
    {
        /// <summary>
        /// Migrates
        /// </summary>
        /// <param name="change">Database model change</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<string> BuildCommand(IModelChange change, CancellationToken token);
    }

    /// <summary>
    /// IModelChangeCommandBuilder
    /// </summary>
    /// <typeparam name="TChange">TChange type-argument</typeparam>
    public interface IModelChangeCommandBuilder<TChange> : IModelChangeCommandBuilder
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