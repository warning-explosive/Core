namespace SpaceEngineers.Core.AuthEndpoint.Host.Registrations
{
    using System.IdentityModel.Tokens.Jwt;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;

    internal class JwtSecurityTokenHandlerManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<JwtSecurityTokenHandler, JwtSecurityTokenHandler>(EnLifestyle.Singleton);
        }
    }
}