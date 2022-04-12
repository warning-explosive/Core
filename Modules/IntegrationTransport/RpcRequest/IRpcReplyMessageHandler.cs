namespace SpaceEngineers.Core.IntegrationTransport.RpcRequest
{
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;

    /// <summary>
    /// IRpcReplyMessageHandler
    /// </summary>
    /// <typeparam name="TReply">TReply type-argument</typeparam>
    public interface IRpcReplyMessageHandler<TReply>
        where TReply : IIntegrationReply
    {
        /// <summary>
        /// Handles incoming RPC-reply
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Handle(IntegrationMessage message, CancellationToken token);
    }
}