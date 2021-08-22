namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using AutoRegistration.Api.Abstractions;
    using Basics.Primitives;

    /// <summary>
    /// IIntegrationUnitOfWork abstraction
    /// </summary>
    public interface IIntegrationUnitOfWork : IAsyncUnitOfWork<IAdvancedIntegrationContext>,
                                              IResolvable
    {
    }
}