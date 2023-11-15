namespace SpaceEngineers.Core.GenericEndpoint.Authorization.Web
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Attributes;
    using Messaging;
    using Microsoft.AspNetCore.Http;

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