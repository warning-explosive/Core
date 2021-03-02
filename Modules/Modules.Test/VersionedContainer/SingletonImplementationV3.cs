namespace SpaceEngineers.Core.Modules.Test.VersionedContainer
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class SingletonImplementationV3 : SingletonImplementation,
                                               IVersionFor<SingletonImplementation>
    {
        public SingletonImplementation Version => this;
    }
}