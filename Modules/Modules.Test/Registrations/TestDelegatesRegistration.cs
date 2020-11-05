namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Enumerations;
    using AutoWiringTest;

    internal class TestDelegatesRegistration : ITestClassWithRegistration
    {
        public void Register(IRegistrationContainer registration)
        {
            registration.Register<IRegisteredByDelegate>(() => (IRegisteredByDelegate)new RegisteredByDelegateImpl(), EnLifestyle.Transient);
            registration.Register<ConcreteRegisteredByDelegate>(() => new ConcreteRegisteredByDelegate(), EnLifestyle.Transient);
        }
    }
}