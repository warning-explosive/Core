namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using CompositionRoot.Api.Abstractions;
    using Messaging;

    /// <summary>
    /// Advanced integration context
    /// Use to pass it in message processing pipeline
    /// </summary>
    public interface IAdvancedIntegrationContext : IIntegrationContext,
                                                   IInitializable<IntegrationMessage>,
                                                   IResolvable
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

        /// <summary>
        /// Delivers built message
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing delivery</returns>
        Task Deliver(IntegrationMessage message, CancellationToken token);

        /// <summary>
        /// Delivers all messages which have buffered during message processing
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing delivery</returns>
        Task DeliverAll(CancellationToken token);
    }
}