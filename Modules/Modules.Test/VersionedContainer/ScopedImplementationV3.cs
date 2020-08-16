namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Scoped)]
    internal class ScopedImplementationV3 : ScopedImplementation,
                                            IVersionFor<ScopedImplementation>
    {
        public ScopedImplementation Version => this;
    }
}