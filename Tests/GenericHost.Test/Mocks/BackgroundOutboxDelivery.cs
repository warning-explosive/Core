namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.UnitOfWork;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;

    [ComponentOverride]
    internal class BackgroundOutboxDelivery : IOutboxDelivery,
                                              IResolvable<IOutboxDelivery>
    {
        public Task DeliverMessages(
            IReadOnlyCollection<IntegrationMessage> messages,
            CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}