namespace SpaceEngineers.Core.IntegrationTransport.RpcRequest
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Pipeline;

    /// <summary>
    /// RpcRequestExtensions
    /// </summary>
    public static class RpcRequestExtensions
    {
        /// <summary>
        /// Request data from target endpoint in blocking manner
        /// </summary>
        /// <param name="integrationContext">Integration context</param>
        /// <param name="request">Integration request</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TRequest">TRequest type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>Ongoing request operation</returns>
        // TODO: #205 - implement http based rpc client (rest | grpc)
        public static async Task<TReply> RpcRequest<TRequest, TReply>(
            this IIntegrationContext integrationContext,
            TRequest request,
            CancellationToken token)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply
        {
            var advancedIntegrationContext = (IAdvancedIntegrationContext)integrationContext;

            var (message, replyTask) = advancedIntegrationContext.EnrollRpcRequest<TRequest, TReply>(request, token);

            if (message == null)
            {
                throw new InvalidOperationException("Rpc request wasn't successful");
            }

            var wasSent = await advancedIntegrationContext
               .SendMessage(message, token)
               .ConfigureAwait(false);

            if (!wasSent)
            {
                throw new InvalidOperationException("Rpc request wasn't successful");
            }

            var reply = await replyTask.ConfigureAwait(false);

            return (TReply)reply.Payload;
        }
    }
}