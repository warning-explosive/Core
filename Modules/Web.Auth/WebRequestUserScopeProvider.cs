namespace SpaceEngineers.Core.Web.Auth
{
    using System.Security.Claims;
    using AuthEndpoint.Contract.Queries;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;
    using GenericEndpoint.Messaging;
    using Microsoft.AspNetCore.Http;

    [Component(EnLifestyle.Scoped)]
    [Before("SpaceEngineers.Core.GenericEndpoint.Messaging.IntegrationMessageUserScopeProvider")]
    internal class WebRequestUserScopeProvider : IUserScopeProvider,
                                                 ICollectionResolvable<IUserScopeProvider>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WebRequestUserScopeProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool TryGetUser(IntegrationMessage? initiatorMessage, out string? user)
        {
            user = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            if (user is not null)
            {
                return true;
            }

            if (initiatorMessage?.Payload is AuthorizeUser authorizeUser)
            {
                user = authorizeUser.Username;
                return true;
            }

            user = default;
            return false;
        }
    }
}