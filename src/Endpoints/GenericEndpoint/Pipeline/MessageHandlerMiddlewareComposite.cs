namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class MessageHandlerMiddlewareComposite : IMessageHandlerMiddlewareComposite,
                                                       IResolvable<IMessageHandlerMiddlewareComposite>
    {
        private readonly MessageHandlerMiddleware _messageHandlerMiddleware;

        public MessageHandlerMiddlewareComposite(IEnumerable<IMessageHandlerMiddleware> messageHandlerMiddlewares)
        {
            _messageHandlerMiddleware = messageHandlerMiddlewares
                .Select(middleware => new MessageHandlerMiddleware(middleware.Handle))
                .Aggregate(static (acc, middleware) => (context, next, token) => acc.Invoke(
                    context,
                    (c, t) => middleware.Invoke(c, next, t),
                    token));
        }

        private delegate Task MessageHandlerMiddleware(
            IAdvancedIntegrationContext context,
            Func<IAdvancedIntegrationContext, CancellationToken, Task> next,
            CancellationToken token);

        public Task Handle(
            IAdvancedIntegrationContext context,
            Func<IAdvancedIntegrationContext, CancellationToken, Task> next,
            CancellationToken token)
        {
            return _messageHandlerMiddleware.Invoke(context, next, token);
        }
    }
}