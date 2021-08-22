namespace SpaceEngineers.Core.CompositionRoot.ManualRegistrations
{
    using Abstractions;
    using Api.Abstractions;

    internal class RegistrationsContainerManualRegistration : IManualRegistration
    {
        private readonly IRegistrationsContainer _registrations;

        public RegistrationsContainerManualRegistration(IRegistrationsContainer registrations)
        {
            _registrations = registrations;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container
                .RegisterInstance<IRegistrationsContainer>(_registrations)
                .RegisterInstance(_registrations.GetType(), _registrations);
        }
    }
}