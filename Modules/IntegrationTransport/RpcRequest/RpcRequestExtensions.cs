namespace SpaceEngineers.Core.IntegrationTransport.RpcRequest
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
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
        /// <param name="query">Integration query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TQuery">TQuery type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>Ongoing request operation</returns>
        public static async Task<TReply> RpcRequest<TQuery, TReply>(
            this IIntegrationContext integrationContext,
            TQuery query,
            CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationReply
        {
            var advancedIntegrationContext = (IAdvancedIntegrationContext)integrationContext;

            var tcs = new TaskCompletionSource<IntegrationMessage>();

            var message = await advancedIntegrationContext
               .TryEnrollRpcRequest<TQuery, TReply>(query, tcs, token)
               .ConfigureAwait(false);

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

            var reply = await tcs.Task.ConfigureAwait(false);

            return (TReply)reply.Payload;
        }
    }
}