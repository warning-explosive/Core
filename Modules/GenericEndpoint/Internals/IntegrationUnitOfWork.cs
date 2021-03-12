namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;

    [Lifestyle(EnLifestyle.Scoped)]
    internal class IntegrationUnitOfWork : AsyncUnitOfWork<IExtendedIntegrationContext>,
                                           IIntegrationUnitOfWork
    {
        private IEnumerable<IUnitOfWorkSubscriber<IExtendedIntegrationContext>> _subscribers;

        public IntegrationUnitOfWork(IEnumerable<IUnitOfWorkSubscriber<IExtendedIntegrationContext>> subscribers)
        {
            _subscribers = subscribers;
        }

        protected override Task OnStart(IExtendedIntegrationContext context, CancellationToken token)
        {
            return ExecuteSubscribers(s => s.OnStart(context, token));
        }

        protected override Task Commit(IExtendedIntegrationContext context, CancellationToken token)
        {
            return ExecuteSubscribers(s => s.OnCommit(context, token));
        }

        protected override Task Rollback(IExtendedIntegrationContext context, CancellationToken token)
        {
            return ExecuteSubscribers(s => s.OnRollback(context, token));
        }

        private Task ExecuteSubscribers(Func<IUnitOfWorkSubscriber<IExtendedIntegrationContext>, Task> accessor)
        {
            return ExecutionExtensions.TryAsync(() => _subscribers.Select(accessor).WhenAll()).Invoke();
        }
    }
}