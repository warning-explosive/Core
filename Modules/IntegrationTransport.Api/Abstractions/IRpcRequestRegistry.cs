namespace SpaceEngineers.Core.IntegrationTransport.Api.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Messaging;

    /// <summary>
    /// IRpcRequestRegistry
    /// </summary>
    public interface IRpcRequestRegistry
    {
        /// <summary>
        /// Enroll RPC request
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
    }
}