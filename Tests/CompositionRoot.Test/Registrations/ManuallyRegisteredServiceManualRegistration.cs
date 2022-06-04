namespace SpaceEngineers.Core.CompositionRoot.Test.Registrations
{
    using Api.Abstractions.Registration;
    using AutoRegistrationTest;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class ManuallyRegisteredServiceManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IManuallyRegisteredService, ManuallyRegisteredService>(EnLifestyle.Transient);
            container.Register<ManuallyRegisteredService, ManuallyRegisteredService>(EnLifestyle.Transient);
        }
    }
}