namespace SpaceEngineers.Core.AuthorizationEndpoint.Host.ManualRegistrations
{
    using CompositionRoot.Api.Abstractions.Registration;
    using JwtAuthentication;

    internal class JwtTokenProviderManualRegistration : IManualRegistration
    {
        private readonly JwtAuthenticationConfiguration _configuration;

        public JwtTokenProviderManualRegistration(JwtAuthenticationConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance<ITokenProvider>(new JwtTokenProvider(_configuration));
        }
    }
}