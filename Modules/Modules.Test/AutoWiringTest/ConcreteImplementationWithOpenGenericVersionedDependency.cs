namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ConcreteImplementationWithOpenGenericVersionedDependency<T> : IResolvable
    {
        public ConcreteImplementationWithOpenGenericVersionedDependency(IVersioned<ConcreteImplementationGenericService<T>> versionedOpenGeneric)
        {
            VersionedOpenGeneric = versionedOpenGeneric;
        }

        public IVersioned<ConcreteImplementationGenericService<T>> VersionedOpenGeneric { get; }
    }
}