namespace SpaceEngineers.Core.IntegrationTransport.Api.Abstractions
{
    using System;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using GenericEndpoint.Contract.Abstractions;

    /// <summary>
    /// IRpcRequestRegistry
    /// </summary>
    public interface IRpcRequestRegistry : IResolvable
    {
        /// <summary>
        /// Enroll RPC request
        /// </summary>
        /// <param name="requestId">Request identifier</param>
        /// <param name="tcs">TaskCompletionSource</param>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>Ongoing enroll operation</returns>
        public Task<bool> TryEnroll<TReply>(Guid requestId, TaskCompletionSource<TReply> tcs)
            where TReply : IIntegrationMessage;

        /// <summary>
        /// Try set RPC request result
        /// </summary>
        /// <param name="requestId">Request identifier</param>
        /// <param name="reply">Reply</param>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>Result</returns>
        bool TrySetResult<TReply>(Guid requestId, TReply reply)
            where TReply : IIntegrationMessage;
    }
}