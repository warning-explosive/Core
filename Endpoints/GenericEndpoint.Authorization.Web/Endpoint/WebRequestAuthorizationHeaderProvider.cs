namespace SpaceEngineers.Core.GenericEndpoint.Authorization.Web.Endpoint
{
    using Basics;
    using Basics.Attributes;
    using GenericEndpoint.Authorization;
    using Messaging;
    using Microsoft.AspNetCore.Http;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using Web;

    [Component(EnLifestyle.Singleton)]
    [Before(typeof(AuthorizationHeaderProvider))]
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
            var authorizationToken = _httpContextAccessor.HttpContext?.GetAuthorizationToken();

            if (!authorizationToken.IsNullOrWhiteSpace() && generalMessage.ReadHeader<Authorization>() == null)
            {
                generalMessage.WriteHeader(new Authorization(authorizationToken));
            }
        }
    }
}