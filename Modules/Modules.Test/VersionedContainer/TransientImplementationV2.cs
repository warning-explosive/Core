namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class TransientImplementationV2 : TransientImplementation,
                                               IVersionFor<TransientImplementation>
    {
        public TransientImplementation Version => this;
    }
}