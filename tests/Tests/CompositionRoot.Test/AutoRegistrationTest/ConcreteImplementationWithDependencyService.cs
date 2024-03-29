namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class ConcreteImplementationWithDependencyService : IResolvable<ConcreteImplementationWithDependencyService>
    {
        public ConcreteImplementationWithDependencyService(ConcreteImplementationService dependency)
        {
            Dependency = dependency;
        }

        public ConcreteImplementationService Dependency { get; }
    }
}