namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Scoped)]
    internal class ScopedVersionedServiceImpl : IScopedVersionedService
    {
    }
}