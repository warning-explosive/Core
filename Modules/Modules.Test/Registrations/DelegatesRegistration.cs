namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Enumerations;
    using AutoWiringTest;

    internal class DelegatesRegistration : IManualRegistration
    {
        public void Register(IRegistrationContainer container)
        {
            container.Register<IRegisteredByDelegate>(() => (IRegisteredByDelegate)new RegisteredByDelegateImpl(), EnLifestyle.Transient);
            container.Register<ConcreteRegisteredByDelegate>(() => new ConcreteRegisteredByDelegate(), EnLifestyle.Transient);
        }
    }
}