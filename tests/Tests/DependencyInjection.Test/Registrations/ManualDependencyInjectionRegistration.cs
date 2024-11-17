namespace SpaceEngineers.Core.DependencyInjection.Test.Registrations;

using Dependencies;
using Registration;

internal class ManualDependencyInjectionRegistration : IDependencyInjectionRegistration
{
    public void Register(IDependencyInjectionRegistrationContainer container)
    {
        container.Register<IManuallyRegisteredService, ManuallyRegisteredService>(EnLifestyle.Transient);
        container.Register<ManuallyRegisteredService, ManuallyRegisteredService>(EnLifestyle.Transient);
    }
}