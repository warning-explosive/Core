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

        protected override Task Start(IExtendedIntegrationContext context, CancellationToken token)
        {
            return ExecuteSubscribers(_subscribers, s => s.OnStart(context, token));
        }

        protected override Task Commit(IExtendedIntegrationContext context, CancellationToken token)
        {
            return ExecuteSubscribers(_subscribers.Reverse(), s => s.OnCommit(context, token));
        }

        protected override Task Rollback(IExtendedIntegrationContext context, Exception? exception, CancellationToken token)
        {
            return ExecuteSubscribers(_subscribers.Reverse(), s => s.OnRollback(context, token));
        }

        private static Task ExecuteSubscribers(
            IEnumerable<IUnitOfWorkSubscriber<IExtendedIntegrationContext>> subscribers,
            Func<IUnitOfWorkSubscriber<IExtendedIntegrationContext>, Task> accessor)
        {
            return subscribers.Select(accessor).WhenAll();
        }
    }
}