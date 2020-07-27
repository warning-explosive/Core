namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ServiceWithVersionedDependenciesImpl : IServiceWithVersionedDependencies
    {
        public ServiceWithVersionedDependenciesImpl(IVersionedService<ITransientVersionedService> transient,
                                                    IVersionedService<IScopedVersionedService> scoped,
                                                    IVersionedService<ISingletonVersionedService> singleton)
        {
            Transient = transient;
            Scoped = scoped;
            Singleton = singleton;
        }

        public IVersionedService<ITransientVersionedService> Transient { get; }

        public IVersionedService<IScopedVersionedService> Scoped { get; }

        public IVersionedService<ISingletonVersionedService> Singleton { get; }
    }
}