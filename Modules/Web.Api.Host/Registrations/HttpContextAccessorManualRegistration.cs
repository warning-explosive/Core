namespace SpaceEngineers.Core.Web.Api.Host.Registrations
{
    using CompositionRoot.Registration;
    using Microsoft.AspNetCore.Http;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;

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
            container.Advanced.RegisterDelegate<IHttpContextAccessor>(
                () => _frameworkDependenciesProvider.GetRequiredService<IHttpContextAccessor>(),
                EnLifestyle.Singleton);
        }
    }
}