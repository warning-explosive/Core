namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class ConcreteImplementationWithDependencyService : IResolvable
    {
        public ConcreteImplementationWithDependencyService(ConcreteImplementationService dependency)
        {
            Dependency = dependency;
        }

        public ConcreteImplementationService Dependency { get; }
    }
}