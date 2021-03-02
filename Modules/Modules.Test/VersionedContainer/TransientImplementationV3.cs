namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class TransientImplementationV3 : TransientImplementation,
                                               IVersionFor<TransientImplementation>
    {
        public TransientImplementation Version => this;
    }
}