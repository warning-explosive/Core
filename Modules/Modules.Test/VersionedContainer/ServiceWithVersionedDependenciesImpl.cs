namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ServiceWithVersionedDependenciesImpl : IServiceWithVersionedDependencies
    {
        public ServiceWithVersionedDependenciesImpl(IVersioned<ITransientVersionedService> transient,
                                                    IVersioned<IScopedVersionedService> scoped,
                                                    IVersioned<ISingletonVersionedService> singleton)
        {
            Transient = transient;
            Scoped = scoped;
            Singleton = singleton;
        }

        public IVersioned<ITransientVersionedService> Transient { get; }

        public IVersioned<IScopedVersionedService> Scoped { get; }

        public IVersioned<ISingletonVersionedService> Singleton { get; }
    }
}