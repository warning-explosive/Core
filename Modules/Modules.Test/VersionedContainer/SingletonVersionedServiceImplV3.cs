namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class SingletonVersionedServiceImplV3 : ISingletonVersionedService,
                                                     IVersionFor<ISingletonVersionedService>
    {
        public ISingletonVersionedService Version => this;
    }
}