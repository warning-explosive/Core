namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class VersionedImplementationV2 : VersionedImplementation,
                                               IVersionFor<VersionedImplementation>
    {
        public VersionedImplementation Version => this;
    }
}