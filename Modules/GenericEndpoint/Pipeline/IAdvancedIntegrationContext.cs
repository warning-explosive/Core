namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using CompositionRoot.Api.Abstractions;
    using Messaging;
    using UnitOfWork;

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
        /// Unit of work
        /// </summary>
        IIntegrationUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// Sends integration message immediately
        /// </summary>
        /// <param name="message">IntegrationMessage</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<bool> SendMessage(IntegrationMessage message, CancellationToken token);

        /// <summary>
        /// Accepts further message processing
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Accept(CancellationToken token);

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
    }
}