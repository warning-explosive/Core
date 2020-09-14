namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Scoped)]
    internal class ScopedVersionedServiceImplV3 : IScopedVersionedService,
                                                  IVersionFor<IScopedVersionedService>
    {
        public IScopedVersionedService Version => this;
    }
}