namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

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