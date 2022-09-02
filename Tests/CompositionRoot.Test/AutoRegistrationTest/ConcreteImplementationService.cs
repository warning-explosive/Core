namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class ConcreteImplementationService : IResolvable<ConcreteImplementationService>
    {
        public ConcreteImplementationService()
        {
        }
    }
}