namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class SingletonVersionedServiceImplV3 : ISingletonVersionedService,
                                                     IVersionFor<ISingletonVersionedService>
    {
        public ISingletonVersionedService Version => this;
    }
}