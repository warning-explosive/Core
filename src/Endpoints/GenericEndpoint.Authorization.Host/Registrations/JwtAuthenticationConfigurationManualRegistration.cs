namespace SpaceEngineers.Core.GenericEndpoint.Authorization.Host.Registrations
{
    using CompositionRoot.Registration;
    using JwtAuthentication;

    internal class JwtAuthenticationConfigurationManualRegistration : IManualRegistration
    {
        private readonly JwtAuthenticationConfiguration _configuration;

        public JwtAuthenticationConfigurationManualRegistration(JwtAuthenticationConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(_configuration);
        }
    }
}