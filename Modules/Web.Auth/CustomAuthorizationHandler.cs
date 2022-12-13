namespace SpaceEngineers.Core.Web.Auth
{
    using System.Threading;
    using System.Threading.Tasks;
    using AuthEndpoint.Contract.Queries;
    using AuthEndpoint.Contract.Replies;
    using GenericEndpoint.Api.Abstractions;
    using IntegrationTransport;
    using IntegrationTransport.RpcRequest;
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

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CustomAuthorizationRequirement requirement)
        {
            var httpContext = (context.Resource as AuthorizationFilterContext)?.HttpContext;

            if (httpContext == null)
            {
                return;
            }

            var username = context.User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            if (!httpContext.Request.RouteValues.TryGetValue("controller", out var controllerName)
                || controllerName is not string controller)
            {
                return;
            }

            if (!httpContext.Request.RouteValues.TryGetValue("action", out var actionName)
                || actionName is not string action)
            {
                return;
            }

            var verb = httpContext.Request.Method;

            var requiredFeatures = _transportDependencyContainer
                .DependencyContainer
                .Resolve<IWebApiFeaturesProvider>()
                .GetFeatures(controller, action, verb);

            UserAuthorizationResult userAuthorizationResult;

            await using (_transportDependencyContainer.DependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                userAuthorizationResult = await _transportDependencyContainer
                    .DependencyContainer
                    .Resolve<IIntegrationContext>()
                    .RpcRequest<AuthorizeUser, UserAuthorizationResult>(new AuthorizeUser(username, $"[{verb}] {controller}/{action}", requiredFeatures), CancellationToken.None)
                    .ConfigureAwait(false);
            }

            if (userAuthorizationResult.AccessGranted)
            {
                context.Succeed(requirement);
            }
        }
    }
}