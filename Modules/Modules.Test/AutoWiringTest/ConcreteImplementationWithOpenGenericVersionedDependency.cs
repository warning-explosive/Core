namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

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