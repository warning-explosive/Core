namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.DataAccess.BackgroundWorkers;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.UnitOfWork;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;

    [ComponentOverride]
    internal class BackgroundOutboxDelivery : IOutboxDelivery,
                                              IDecorator<IOutboxDelivery>
    {
        public BackgroundOutboxDelivery(IOutboxDelivery decoratee)
        {
            Decoratee = decoratee;
        }

        public IOutboxDelivery Decoratee { get; }

        public Task DeliverMessages(
            IReadOnlyCollection<IntegrationMessage> messages,
            CancellationToken token)
        {
            if (!Environment.StackTrace.Contains(nameof(GenericEndpointDataAccessHostBackgroundWorker), StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            return Decoratee.DeliverMessages(messages, token);
        }
    }
}