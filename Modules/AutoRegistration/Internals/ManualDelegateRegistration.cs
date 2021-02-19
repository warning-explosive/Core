namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using Abstractions;

    internal class ManualDelegateRegistration : IManualRegistration
    {
        private readonly Action<IRegistrationContainer> _registrationAction;

        public ManualDelegateRegistration(Action<IRegistrationContainer> registrationAction)
        {
            _registrationAction = registrationAction;
        }

        public void Register(IRegistrationContainer container)
        {
            _registrationAction(container);
        }
    }
}