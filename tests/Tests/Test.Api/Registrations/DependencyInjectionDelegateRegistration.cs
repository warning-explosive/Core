namespace SpaceEngineers.Core.Test.Api.Registrations;

using System;
using DependencyInjection.Registration;

internal class DependencyInjectionDelegateRegistration : IDependencyInjectionRegistration
{
    private readonly Action<IDependencyInjectionRegistrationContainer> _registrationAction;

    public DependencyInjectionDelegateRegistration(Action<IDependencyInjectionRegistrationContainer> registrationAction)
    {
        _registrationAction = registrationAction;
    }

    public void Register(IDependencyInjectionRegistrationContainer container)
    {
        _registrationAction(container);
    }

    public override int GetHashCode()
    {
        return _registrationAction.GetHashCode();
    }
}