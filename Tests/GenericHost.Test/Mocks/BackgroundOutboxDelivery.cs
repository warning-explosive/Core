namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
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
            return Environment.StackTrace.Contains(nameof(GenericEndpoint.DataAccess.UnitOfWork.IntegrationUnitOfWork), StringComparison.OrdinalIgnoreCase)
                ? Task.CompletedTask
                : Decoratee.DeliverMessages(messages, token);
        }
    }
}