namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Scoped)]
    internal class ScopedVersionedServiceImplV2 : IScopedVersionedService,
                                                  IVersionFor<IScopedVersionedService>
    {
        public IScopedVersionedService Version => this;
    }
}