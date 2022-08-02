namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using CompositionRoot.Api.Abstractions;
    using Contract.Abstractions;
    using Messaging;

    /// <summary>
    /// Advanced integration context
    /// Use to pass it in message processing pipeline
    /// </summary>
    public interface IAdvancedIntegrationContext : IIntegrationContext,
                                                   IInitializable<IntegrationMessage>
    {
        /// <summary>
        /// Integration message, processing initiator
        /// </summary>
        IntegrationMessage Message { get; }

        /// <summary>
        /// Immediately sends integration message
        /// </summary>
        /// <param name="message">IntegrationMessage</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<bool> SendMessage(IntegrationMessage message, CancellationToken token);

        /// <summary>
        /// Rejects further message processing and moves message to errors
        /// </summary>
        /// <param name="exception">Processing error</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Reject(Exception exception, CancellationToken token);

        /// <summary>
        /// Retries integration message processing using specified retry policy
        /// </summary>
        /// <param name="dueTime">Time that transport waits before deliver message again</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Retry(TimeSpan dueTime, CancellationToken token);

        /// <summary>
        /// Try enroll rpc-request
        /// </summary>
        /// <param name="query">Integration query</param>
        /// <param name="tcs">TaskCompletionSource</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TQuery">TQuery type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>Ongoing enroll operation</returns>
        Task<IntegrationMessage?> TryEnrollRpcRequest<TQuery, TReply>(
            TQuery query,
            TaskCompletionSource<IntegrationMessage> tcs,
            CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationReply;
    }
}