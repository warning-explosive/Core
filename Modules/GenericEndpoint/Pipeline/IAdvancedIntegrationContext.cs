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
        /// Produces message to transport again and retries integration message processing using specified retry policy
        /// Should be called within endpoint scope (in message handler)
        /// </summary>
        /// <param name="dueTime">Time that transport waits before deliver message again</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing retry operation</returns>
        Task Retry(TimeSpan dueTime, CancellationToken token);

        /// <summary>
        /// Refuses further message processing and moves message to errors
        /// </summary>
        /// <param name="exception">Processing error</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing reject operation</returns>
        Task Refuse(Exception exception, CancellationToken token);
    }
}