namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ConcreteImplementationWithOpenGenericVersionedDependency<T> : IResolvable
    {
        private IVersioned<ConcreteImplementationGenericService<T>> _versionedOpenGeneric;

        public ConcreteImplementationWithOpenGenericVersionedDependency(IVersioned<ConcreteImplementationGenericService<T>> versionedOpenGeneric)
        {
            _versionedOpenGeneric = versionedOpenGeneric;
        }
    }
}