namespace SpaceEngineers.Core.GenericEndpoint.Web.Api.Host.Registrations
{
    using Microsoft.AspNetCore.Http;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using GenericHost;

    internal class HttpContextAccessorManualRegistration : IManualRegistration
    {
        private readonly IFrameworkDependenciesProvider _frameworkDependenciesProvider;

        public HttpContextAccessorManualRegistration(
            IFrameworkDependenciesProvider frameworkDependenciesProvider)
        {
            _frameworkDependenciesProvider = frameworkDependenciesProvider;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterDelegate(
                () => _frameworkDependenciesProvider.GetRequiredService<IHttpContextAccessor>(),
                EnLifestyle.Singleton);
        }
    }
}