namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class SingletonImplementationV2 : SingletonImplementation,
                                               IVersionFor<SingletonImplementation>
    {
        public SingletonImplementation Version => this;
    }
}