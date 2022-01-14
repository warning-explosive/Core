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
            var tokenProvider = new JwtTokenProvider(_configuration);

            container.RegisterInstance<ITokenProvider>(tokenProvider);
            container.RegisterInstance<JwtTokenProvider>(tokenProvider);
        }
    }
}