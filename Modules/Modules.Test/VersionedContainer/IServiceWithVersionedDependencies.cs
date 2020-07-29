namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;

    internal interface IServiceWithVersionedDependencies : IResolvable
    {
        IVersioned<ITransientVersionedService> Transient { get; }

        IVersioned<IScopedVersionedService> Scoped { get; }

        IVersioned<ISingletonVersionedService> Singleton { get; }
    }
}