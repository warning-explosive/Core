namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class ConcreteImplementationWithDependencyService : IResolvableImplementation
    {
        public ConcreteImplementationWithDependencyService(ConcreteImplementationService dependency)
        {
            Dependency = dependency;
        }

        public ConcreteImplementationService Dependency { get; }
    }
}