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
    using Basics.Primitives;

    [Component(EnLifestyle.Scoped)]
    internal class IntegrationUnitOfWork : AsyncUnitOfWork<IAdvancedIntegrationContext>,
                                           IIntegrationUnitOfWork
    {
        private readonly IEnumerable<IUnitOfWorkSubscriber<IAdvancedIntegrationContext>> _subscribers;

        public IntegrationUnitOfWork(IEnumerable<IUnitOfWorkSubscriber<IAdvancedIntegrationContext>> subscribers)
        {
            _subscribers = subscribers;
        }

        protected override Task Start(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return ExecuteSubscribers(_subscribers, s => s.OnStart(context, token));
        }

        protected override Task Commit(IAdvancedIntegrationContext context, CancellationToken token)
        {
            return ExecuteSubscribers(_subscribers.Reverse(), s => s.OnCommit(context, token));
        }

        protected override Task Rollback(IAdvancedIntegrationContext context, Exception? exception, CancellationToken token)
        {
            return ExecuteSubscribers(_subscribers.Reverse(), s => s.OnRollback(context, token));
        }

        private static Task ExecuteSubscribers(
            IEnumerable<IUnitOfWorkSubscriber<IAdvancedIntegrationContext>> subscribers,
            Func<IUnitOfWorkSubscriber<IAdvancedIntegrationContext>, Task> accessor)
        {
            return subscribers.Select(accessor).WhenAll();
        }
    }
}