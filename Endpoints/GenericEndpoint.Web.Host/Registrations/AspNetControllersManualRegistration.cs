namespace SpaceEngineers.Core.GenericEndpoint.Web.Host.Registrations
{
    using System.Linq;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;

    internal class AspNetControllersManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            var controllers = container
                .Types
                .OurTypes
                .Where(type => type.IsController());

            foreach (var controller in controllers)
            {
                container.Register(controller, controller, EnLifestyle.Transient);
            }
        }
    }
}