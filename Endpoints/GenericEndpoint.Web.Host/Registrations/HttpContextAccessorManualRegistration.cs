namespace SpaceEngineers.Core.GenericEndpoint.Web.Host.Registrations
{
    using CompositionRoot.Registration;
    using GenericHost;
    using Microsoft.AspNetCore.Http;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class HttpContextAccessorManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterDelegate(
                () => container
                    .Advanced
                    .DependencyContainer
                    .Resolve<IFrameworkDependenciesProvider>()
                    .GetRequiredService<IHttpContextAccessor>(),
                EnLifestyle.Singleton);
        }
    }
}