namespace SpaceEngineers.Core.GenericEndpoint.UnitOfWork
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    internal interface IOutboxDelivery
    {
        Task DeliverMessages(
            IReadOnlyCollection<IntegrationMessage> messages,
            CancellationToken token);
    }
}