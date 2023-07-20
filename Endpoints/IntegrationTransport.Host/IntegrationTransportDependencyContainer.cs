namespace SpaceEngineers.Core.IntegrationTransport.Host
{
    using CompositionRoot;

    internal class IntegrationTransportDependencyContainer
    {
        public IntegrationTransportDependencyContainer(IDependencyContainer dependencyContainer)
        {
            DependencyContainer = dependencyContainer;
        }

        public IDependencyContainer DependencyContainer { get; }
    }
}