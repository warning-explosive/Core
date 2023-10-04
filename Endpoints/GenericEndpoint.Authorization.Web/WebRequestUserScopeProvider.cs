namespace SpaceEngineers.Core.GenericEndpoint.Authorization.Web
{
    using System.Security.Claims;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using Messaging;
    using Messaging.MessageHeaders;
    using Microsoft.AspNetCore.Http;

    [Component(EnLifestyle.Singleton)]
    [Before(typeof(UserScopeProvider))]
    internal class WebRequestUserScopeProvider : IIntegrationMessageHeaderProvider,
                                                 ICollectionResolvable<IIntegrationMessageHeaderProvider>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WebRequestUserScopeProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void WriteHeaders(IntegrationMessage generalMessage, IntegrationMessage? initiatorMessage)
        {
            var user = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            if (user != null && generalMessage.ReadHeader<User>() == null)
            {
                generalMessage.WriteHeader(new User(user));
            }
        }
    }
}