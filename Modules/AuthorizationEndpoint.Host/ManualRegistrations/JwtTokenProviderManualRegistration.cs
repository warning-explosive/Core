namespace SpaceEngineers.Core.AuthorizationEndpoint.Host.ManualRegistrations
{
    using AutoRegistration.Api.Enumerations;
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
            container.Advanced.RegisterDelegate(() => new JwtTokenProvider(_configuration), EnLifestyle.Singleton);
        }
    }
}