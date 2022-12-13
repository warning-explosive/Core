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

    [Component(EnLifestyle.Singleton)]
    [After(typeof(UnitOfWorkMiddleware))]
    internal class HandledByEndpointMiddleware : IMessageHandlerMiddleware,
                                                 ICollectionResolvable<IMessageHandlerMiddleware>
    {
        private readonly EndpointIdentity _endpointIdentity;

        public HandledByEndpointMiddleware(EndpointIdentity endpointIdentity)
        {
            _endpointIdentity = endpointIdentity;
        }

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
                context.Message.OverwriteHeader(new HandledBy(_endpointIdentity));
            }
        }
    }
}