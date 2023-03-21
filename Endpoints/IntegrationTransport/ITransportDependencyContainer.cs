namespace SpaceEngineers.Core.IntegrationTransport
{
    using CompositionRoot;

    /// <summary>
    /// ITransportDependencyContainer
    /// </summary>
    public interface ITransportDependencyContainer
    {
        /// <summary>
        /// IDependencyContainer
        /// </summary>
        IDependencyContainer DependencyContainer { get; }
    }
}