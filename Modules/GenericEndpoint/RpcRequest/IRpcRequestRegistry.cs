namespace SpaceEngineers.Core.GenericEndpoint.RpcRequest
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    /// IRpcRequestRegistry
    /// </summary>
    public interface IRpcRequestRegistry
    {
        /// <summary>
        /// Enroll rpc-request
        /// </summary>
        /// <param name="requestId">Request identifier</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing enroll operation</returns>
        public Task<IntegrationMessage> Enroll(Guid requestId, CancellationToken token);

        /// <summary>
        /// Try set RPC request result
        /// </summary>
        /// <param name="requestId">Request identifier</param>
        /// <param name="reply">Reply</param>
        /// <returns>Result</returns>
        bool TrySetResult(Guid requestId, IntegrationMessage reply);

        /// <summary>
        /// Try set RPC request exception
        /// </summary>
        /// <param name="requestId">Request identifier</param>
        /// <param name="exception">Exception</param>
        /// <returns>Result</returns>
        bool TrySetException(Guid requestId, Exception exception);
    }
}