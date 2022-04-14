namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.UnitOfWork
{
    using System.Threading;
    using System.Threading.Tasks;
    using Deduplication;

    internal interface IOutboxMessagesDelivery
    {
        Task DeliverMessages(Outbox outbox, CancellationToken token);
    }
}