namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class TransientVersionedServiceImplV2 : ITransientVersionedService,
                                                     IVersionFor<ITransientVersionedService>
    {
        public ITransientVersionedService Version => this;
    }
}