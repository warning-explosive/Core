namespace SpaceEngineers.Core.GenericEndpoint.Host
{
    using CompositionRoot;

    internal class GenericEndpointDependencyContainer
    {
        public GenericEndpointDependencyContainer(IDependencyContainer dependencyContainer)
        {
            DependencyContainer = dependencyContainer;
        }

        public IDependencyContainer DependencyContainer { get; }
    }
}