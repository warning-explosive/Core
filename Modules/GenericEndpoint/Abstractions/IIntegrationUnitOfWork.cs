namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using AutoWiring.Api.Abstractions;
    using Basics.Primitives;

    /// <summary>
    /// IIntegrationUnitOfWork abstraction
    /// </summary>
    public interface IIntegrationUnitOfWork : IAsyncUnitOfWork<IAdvancedIntegrationContext>,
                                              IResolvable
    {
    }
}