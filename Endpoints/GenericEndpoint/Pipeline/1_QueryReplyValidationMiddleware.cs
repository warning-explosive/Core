namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using CompositionRoot;
    using Messaging.Extensions;
    using UnitOfWork;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(HandledByEndpointMiddleware))]
    internal class QueryReplyValidationMiddleware : IMessageHandlerMiddleware,
                                                    ICollectionResolvable<IMessageHandlerMiddleware>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public QueryReplyValidationMiddleware(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task Handle(
            IAdvancedIntegrationContext context,
            Func<IAdvancedIntegrationContext, CancellationToken, Task> next,
            CancellationToken token)
        {
            await next
                .Invoke(context, token)
                .ConfigureAwait(false);

            if (context.Message.IsQuery())
            {
                var repliesCount = _dependencyContainer
                   .Resolve<IOutboxStorage>()
                   .All()
                   .Count(message => message.IsReplyOnQuery(context.Message));

                switch (repliesCount)
                {
                    case < 1: throw new InvalidOperationException("Message handler should reply to the query");
                    case > 1: throw new InvalidOperationException("Message handler should reply to the query only once");
                }
            }
        }
    }
}