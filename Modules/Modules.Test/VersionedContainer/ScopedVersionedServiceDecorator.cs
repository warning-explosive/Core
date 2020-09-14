namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Scoped)]
    internal class ScopedVersionedServiceDecorator : IScopedVersionedService,
                                                     IDecorator<IScopedVersionedService>
    {
        public ScopedVersionedServiceDecorator(IScopedVersionedService decoratee)
        {
            Decoratee = decoratee;
        }

        public IScopedVersionedService Decoratee { get; }
    }
}