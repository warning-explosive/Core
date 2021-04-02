namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class IntegrationContextUnitOfWorkSubscriber : IUnitOfWorkSubscriber<IAdvancedIntegrationContext>
    {
        public Task OnStart(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task OnCommit(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return context.DeliverAll(token);
        }

        public Task OnRollback(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}