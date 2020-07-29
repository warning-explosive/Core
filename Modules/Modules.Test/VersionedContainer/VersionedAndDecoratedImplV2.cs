namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class VersionedAndDecoratedImplV2 : IVersionedAndDecorated,
                                                 IVersionFor<IVersionedAndDecorated>
    {
        public IVersionedAndDecorated Version => this;
    }
}