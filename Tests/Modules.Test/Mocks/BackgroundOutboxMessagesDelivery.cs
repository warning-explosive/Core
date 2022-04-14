namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using GenericEndpoint.DataAccess.Deduplication;
    using GenericEndpoint.DataAccess.UnitOfWork;

    [ComponentOverride]
    internal class BackgroundOutboxMessagesDelivery : IOutboxMessagesDelivery,
                                                      IResolvable<IOutboxMessagesDelivery>
    {
        public Task DeliverMessages(Outbox outbox, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}