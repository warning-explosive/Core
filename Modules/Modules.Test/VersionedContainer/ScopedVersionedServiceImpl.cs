namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Scoped)]
    internal class ScopedVersionedServiceImpl : IScopedVersionedService
    {
    }
}