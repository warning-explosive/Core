namespace SpaceEngineers.Core.CompositionRoot.Test.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using AutoRegistrationTest;
    using Registration;

    internal class ManuallyRegisteredServiceManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IManuallyRegisteredService, ManuallyRegisteredService>(EnLifestyle.Transient);
            container.Register<ManuallyRegisteredService, ManuallyRegisteredService>(EnLifestyle.Transient);
        }
    }
}