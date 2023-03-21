namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.UnitOfWork
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IOutboxBackgroundDelivery
    /// </summary>
    public interface IOutboxBackgroundDelivery
    {
        /// <summary>
        /// Delivers messages
        /// </summary>
        /// <param name="token">token</param>
        /// <returns>Ongoing operation</returns>
        Task DeliverMessages(CancellationToken token);
    }
}