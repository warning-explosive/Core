namespace SpaceEngineers.Core.Test.Api.Internals
{
    using System;
    using CompositionRoot.Api.Abstractions;

    internal class ManualDelegateRegistration : IManualRegistration
    {
        private readonly Action<IManualRegistrationsContainer> _registrationAction;

        public ManualDelegateRegistration(Action<IManualRegistrationsContainer> registrationAction)
        {
            _registrationAction = registrationAction;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            _registrationAction(container);
        }

        public override int GetHashCode()
        {
            return _registrationAction.GetHashCode();
        }
    }
}