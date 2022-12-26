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

    /// <summary>
    /// RequestReplyValidationMiddleware
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    [After(typeof(HandledByEndpointMiddleware))]
    public class RequestReplyValidationMiddleware : IMessageHandlerMiddleware,
                                                    ICollectionResolvable<IMessageHandlerMiddleware>
    {
        private readonly IDependencyContainer _dependencyContainer;

        /// <summary> .cctor </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        public RequestReplyValidationMiddleware(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        /// <inheritdoc />
        public async Task Handle(
            IAdvancedIntegrationContext context,
            Func<IAdvancedIntegrationContext, CancellationToken, Task> next,
            CancellationToken token)
        {
            await next
                .Invoke(context, token)
                .ConfigureAwait(false);

            if (context.Message.IsRequest())
            {
                var repliesCount = _dependencyContainer
                   .Resolve<IOutboxStorage>()
                   .All()
                   .Count(message => message.IsReplyOnRequest(context.Message));

                switch (repliesCount)
                {
                    case < 1: throw new InvalidOperationException("Message handler should provide a reply in response on incoming request");
                    case > 1: throw new InvalidOperationException("Message handler should provide a reply in response on incoming request only once");
                }
            }
        }
    }
}