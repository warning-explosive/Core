namespace SpaceEngineers.Core.IntegrationTransport
{
    using CompositionRoot;

    internal class TransportDependencyContainer : ITransportDependencyContainer
    {
        public TransportDependencyContainer(IDependencyContainer dependencyContainer)
        {
            DependencyContainer = dependencyContainer;
        }

        public IDependencyContainer DependencyContainer { get; }
    }
}