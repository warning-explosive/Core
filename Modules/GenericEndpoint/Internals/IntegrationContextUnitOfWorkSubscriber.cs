namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Scoped)]
    internal class IntegrationContextUnitOfWorkSubscriber : IUnitOfWorkSubscriber<IExtendedIntegrationContext>
    {
        public Task OnStart(IExtendedIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task OnCommit(IExtendedIntegrationContext context, CancellationToken token)
        {
            return context.DeliverAll(token);
        }

        public Task OnRollback(IExtendedIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}