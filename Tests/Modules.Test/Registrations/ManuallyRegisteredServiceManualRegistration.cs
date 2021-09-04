namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using AutoRegistrationTest;
    using CompositionRoot.Api.Abstractions.Registration;

    internal class ManuallyRegisteredServiceManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IManuallyRegisteredService, ManuallyRegisteredService>(EnLifestyle.Transient);
            container.Register<ManuallyRegisteredService, ManuallyRegisteredService>(EnLifestyle.Transient);
        }
    }
}