namespace SpaceEngineers.Core.DependencyInjection.Registration;

public interface IDependencyInjectionRegistration
{
    public void Register(IDependencyInjectionRegistrationContainer container);
}