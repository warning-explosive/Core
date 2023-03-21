namespace SpaceEngineers.Core.Web.Auth
{
    using System.Linq;
    using System.Threading.Tasks;
    using Basics;
    using IntegrationTransport;
    using JwtAuthentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Filters;

    internal class CustomAuthorizationHandler : AuthorizationHandler<CustomAuthorizationRequirement>
    {
        private readonly ITransportDependencyContainer _transportDependencyContainer;

        public CustomAuthorizationHandler(
            ITransportDependencyContainer transportDependencyContainer)
        {
            _transportDependencyContainer = transportDependencyContainer;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CustomAuthorizationRequirement requirement)
        {
            var httpContext = (context.Resource as AuthorizationFilterContext)?.HttpContext;

            if (httpContext == null)
            {
                return Task.CompletedTask;
            }

            var username = context.User.Identity?.Name;

            if (username.IsNullOrWhiteSpace())
            {
                return Task.CompletedTask;
            }

            var authorizationToken = httpContext.GetAuthorizationToken();

            if (authorizationToken.IsNullOrWhiteSpace())
            {
                return Task.CompletedTask;
            }

            var grantedPermissions = _transportDependencyContainer
                .DependencyContainer
                .Resolve<ITokenProvider>()
                .GetPermissions(authorizationToken);

            if (!httpContext.Request.RouteValues.TryGetValue("controller", out var controllerName)
                || controllerName is not string controller)
            {
                return Task.CompletedTask;
            }

            if (!httpContext.Request.RouteValues.TryGetValue("action", out var actionName)
                || actionName is not string action)
            {
                return Task.CompletedTask;
            }

            var verb = httpContext.Request.Method;

            var requiredFeatures = _transportDependencyContainer
                .DependencyContainer
                .Resolve<IWebApiFeaturesProvider>()
                .GetFeatures(controller, action, verb);

            var accessGranted = requiredFeatures.All(requiredFeature => grantedPermissions.Contains(requiredFeature));

            if (accessGranted)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}