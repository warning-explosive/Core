namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;

    internal interface IServiceWithVersionedDependencies : IResolvable
    {
        IVersionedService<ITransientVersionedService> Transient { get; }

        IVersionedService<IScopedVersionedService> Scoped { get; }

        IVersionedService<ISingletonVersionedService> Singleton { get; }
    }
}