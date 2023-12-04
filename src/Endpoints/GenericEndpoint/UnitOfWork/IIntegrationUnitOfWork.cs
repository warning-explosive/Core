namespace SpaceEngineers.Core.GenericEndpoint.UnitOfWork
{
    using Basics.Primitives;
    using Pipeline;

    /// <summary>
    /// IIntegrationUnitOfWork abstraction
    /// </summary>
    public interface IIntegrationUnitOfWork : IAsyncUnitOfWork<IAdvancedIntegrationContext>
    {
    }
}