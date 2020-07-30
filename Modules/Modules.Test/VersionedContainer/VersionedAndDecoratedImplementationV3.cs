namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class VersionedAndDecoratedImplementationV3 : VersionedAndDecoratedImplementation,
                                                           IVersionFor<VersionedAndDecoratedImplementation>
    {
        public VersionedAndDecoratedImplementation Version => this;
    }
}