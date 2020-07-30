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
                                                    IVersioned<ISingletonVersionedService> singleton,
                                                    IVersioned<TransientImplementation> transientImplementation,
                                                    IVersioned<ScopedImplementation> scopedImplementation,
                                                    IVersioned<SingletonImplementation> singletonImplementation)
        {
            Transient = transient;
            Scoped = scoped;
            Singleton = singleton;
            TransientImplementation = transientImplementation;
            ScopedImplementation = scopedImplementation;
            SingletonImplementation = singletonImplementation;
        }

        public IVersioned<ITransientVersionedService> Transient { get; }

        public IVersioned<IScopedVersionedService> Scoped { get; }

        public IVersioned<ISingletonVersionedService> Singleton { get; }

        public IVersioned<TransientImplementation> TransientImplementation { get; }

        public IVersioned<ScopedImplementation> ScopedImplementation { get; }

        public IVersioned<SingletonImplementation> SingletonImplementation { get; }
    }
}