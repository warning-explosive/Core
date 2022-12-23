namespace SpaceEngineers.Core.Web.Auth
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using GenericEndpoint.Authorization;
    using GenericEndpoint.Messaging;
    using Microsoft.AspNetCore.Http;

    [Component(EnLifestyle.Scoped)]
    [Before("SpaceEngineers.Core.GenericEndpoint.Authorization SpaceEngineers.Core.GenericEndpoint.Authorization.AuthorizationHeaderProvider")]
    internal class WebRequestAuthorizationHeaderProvider : IIntegrationMessageHeaderProvider,
                                                           ICollectionResolvable<IIntegrationMessageHeaderProvider>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WebRequestAuthorizationHeaderProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void WriteHeaders(IntegrationMessage generalMessage, IntegrationMessage? initiatorMessage)
        {
            var token = _httpContextAccessor.HttpContext?.GetAuthorizationToken();

            if (token != null && generalMessage.ReadHeader<Authorization>() == null)
            {
                generalMessage.WriteHeader(new Authorization(token));
            }
        }
    }
}