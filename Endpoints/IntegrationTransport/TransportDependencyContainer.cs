namespace SpaceEngineers.Core.IntegrationTransport
{
    using CompositionRoot;

    // TODO: #225 - allow several transports per process (even of the same type), make up ow to route outgoing messages automatically
    // TODO: #225 - rethink transport as endpoint approach, move transport into separate assembly
    internal class TransportDependencyContainer : ITransportDependencyContainer
    {
        public TransportDependencyContainer(IDependencyContainer dependencyContainer)
        {
            DependencyContainer = dependencyContainer;
        }

        public IDependencyContainer DependencyContainer { get; }
    }
}