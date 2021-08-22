namespace SpaceEngineers.Core.GenericEndpoint.Api
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Abstractions;
    using Contract.Abstractions;

    /// <summary>
    /// Message handler abstraction
    /// Implements reaction on incoming messages
    /// </summary>
    /// <typeparam name="TMessage">TMessage type-argument</typeparam>
    public abstract class MessageHandlerBase<TMessage> : IMessageHandler<TMessage>,
                                                         ICollectionResolvable<IMessageHandler<TMessage>>
        where TMessage : IIntegrationMessage
    {
        /// <inheritdoc />
        public abstract Task Handle(TMessage message, IIntegrationContext context, CancellationToken token);
    }
}