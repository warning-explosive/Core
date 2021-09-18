namespace SpaceEngineers.Core.GenericEndpoint.UnitOfWork
{
    using AutoRegistration.Api.Abstractions;
    using Basics.Primitives;
    using Pipeline;

    /// <summary>
    /// IIntegrationUnitOfWork abstraction
    /// </summary>
    public interface IIntegrationUnitOfWork : IAsyncUnitOfWork<IAdvancedIntegrationContext>,
                                              IResolvable
    {
        /// <summary>
        /// Transactional outbox storage
        /// </summary>
        IOutboxStorage OutboxStorage { get; }
    }
}