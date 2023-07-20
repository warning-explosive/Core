namespace SpaceEngineers.Core.GenericEndpoint.Authorization.Web.Endpoint
{
    using System.Security.Claims;
    using Basics.Attributes;
    using Messaging;
    using Messaging.MessageHeaders;
    using Microsoft.AspNetCore.Http;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    [Before("SpaceEngineers.Core.GenericEndpoint.Messaging SpaceEngineers.Core.GenericEndpoint.Messaging.UserScopeProvider")]
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