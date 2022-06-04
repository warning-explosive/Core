namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

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