namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class SingletonVersionedServiceImpl : ISingletonVersionedService
    {
    }
}