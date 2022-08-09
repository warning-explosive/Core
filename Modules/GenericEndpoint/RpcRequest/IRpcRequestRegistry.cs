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
        /// Try enroll rpc-request
        /// </summary>
        /// <param name="requestId">Request identifier</param>
        /// <param name="tcs">TaskCompletionSource</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing enroll operation</returns>
        public Task<bool> TryEnroll(Guid requestId, TaskCompletionSource<IntegrationMessage> tcs, CancellationToken token);

        /// <summary>
        /// Try set RPC request result
        /// </summary>
        /// <param name="requestId">Request identifier</param>
        /// <param name="reply">Reply</param>
        /// <returns>Result</returns>
        bool TrySetResult(Guid requestId, IntegrationMessage reply);

        /// <summary>
        /// Try set cancelled state to RPC request
        /// </summary>
        /// <param name="requestId">Request identifier</param>
        /// <returns>Result</returns>
        bool TrySetCancelled(Guid requestId);
    }
}