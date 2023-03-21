namespace SpaceEngineers.Core.Web.Api.Host.Registrations
{
    using System.IdentityModel.Tokens.Jwt;
    using CompositionRoot.Registration;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class JwtSecurityTokenHandlerManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<JwtSecurityTokenHandler, JwtSecurityTokenHandler>(EnLifestyle.Singleton);
        }
    }
}