namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class SingletonImplementationV3 : SingletonImplementation,
                                               IVersionFor<SingletonImplementation>
    {
        public SingletonImplementation Version => this;
    }
}