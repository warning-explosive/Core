namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class TransientVersionedServiceImplV2 : ITransientVersionedService,
                                                     IVersionFor<ITransientVersionedService>
    {
        public ITransientVersionedService Version => this;
    }
}