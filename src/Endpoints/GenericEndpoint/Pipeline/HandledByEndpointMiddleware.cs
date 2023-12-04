namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using Contract;
    using Messaging.MessageHeaders;

    /// <summary>
    /// HandledByEndpointMiddleware
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    [After(typeof(UnitOfWorkMiddleware))]
    public class HandledByEndpointMiddleware : IMessageHandlerMiddleware,
                                               ICollectionResolvable<IMessageHandlerMiddleware>
    {
        private readonly EndpointIdentity _endpointIdentity;

        /// <summary> .cctor </summary>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        public HandledByEndpointMiddleware(EndpointIdentity endpointIdentity)
        {
            _endpointIdentity = endpointIdentity;
        }

        /// <inheritdoc />
        public async Task Handle(
            IAdvancedIntegrationContext context,
            Func<IAdvancedIntegrationContext, CancellationToken, Task> next,
            CancellationToken token)
        {
            try
            {
                await next(context, token).ConfigureAwait(false);
            }
            finally
            {
                context.Message.WriteHeader(new HandledBy(_endpointIdentity));
            }
        }
    }
}