namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using AutoWiring.Api.Abstractions;
    using Basics;

    /// <summary>
    /// IIntegrationUnitOfWork abstraction
    /// </summary>
    public interface IIntegrationUnitOfWork : IAsyncUnitOfWork<IExtendedIntegrationContext>,
                                              IResolvable
    {
    }
}