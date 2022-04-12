namespace SpaceEngineers.Core.CompositionRoot.ManualRegistrations
{
    using Api.Abstractions.Registration;
    using Implementations;

    internal class RegistrationsContainerManualRegistration : IManualRegistration
    {
        private readonly CompositeRegistrationsContainer _registrations;
        private readonly ComponentsOverrideContainer _overrides;

        public RegistrationsContainerManualRegistration(
            CompositeRegistrationsContainer registrations,
            ComponentsOverrideContainer overrides)
        {
            _registrations = registrations;
            _overrides = overrides;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container
                .RegisterInstance<IRegistrationsContainer>(_registrations)
                .RegisterInstance<IComponentsOverrideContainer>(_overrides);
        }
    }
}