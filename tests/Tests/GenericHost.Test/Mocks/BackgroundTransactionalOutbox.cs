namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.UnitOfWork;
    using IntegrationUnitOfWork = GenericEndpoint.DataAccess.Sql.UnitOfWork.IntegrationUnitOfWork;

    [ComponentOverride]
    internal class BackgroundTransactionalOutbox : ITransactionalOutbox,
                                                   IDecorator<ITransactionalOutbox>
    {
        public BackgroundTransactionalOutbox(ITransactionalOutbox decoratee)
        {
            Decoratee = decoratee;
        }

        public ITransactionalOutbox Decoratee { get; }

        public void Add(IntegrationMessage message)
        {
            Decoratee.Add(message);
        }

        public IReadOnlyCollection<IntegrationMessage> All()
        {
            return Decoratee.All();
        }

        public Task DeliverMessages(CancellationToken token)
        {
            return Environment.StackTrace.Contains(nameof(IntegrationUnitOfWork), StringComparison.OrdinalIgnoreCase)
                ? Task.CompletedTask
                : Decoratee.DeliverMessages(token);
        }
    }
}