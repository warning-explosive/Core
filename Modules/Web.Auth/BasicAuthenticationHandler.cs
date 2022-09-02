namespace SpaceEngineers.Core.Web.Auth
{
    using System;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading;
    using System.Threading.Tasks;
    using AuthEndpoint.Contract.Queries;
    using AuthEndpoint.Contract.Replies;
    using Basics;
    using Extensions;
    using GenericEndpoint.Api.Abstractions;
    using IntegrationTransport;
    using IntegrationTransport.RpcRequest;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    internal class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ITransportDependencyContainer _transportDependencyContainer;

        public BasicAuthenticationHandler(
            ITransportDependencyContainer transportDependencyContainer,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _transportDependencyContainer = transportDependencyContainer;
        }

        protected sealed override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();

            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                return AuthenticateResult.NoResult();
            }

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing authorization header");
            }

            var authenticationHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);

            if (!authenticationHeader.Scheme.Equals(BasicDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.NoResult();
            }

            if (authenticationHeader.Parameter == null)
            {
                return AuthenticateResult.Fail("Invalid authorization header");
            }

            var (username, password) = authenticationHeader.Parameter.DecodeBasicAuth();

            UserAuthorizationResult authorizationResult;

            var dependencyContainer = _transportDependencyContainer.DependencyContainer;

            await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                authorizationResult = await dependencyContainer
                   .Resolve<IIntegrationContext>()
                   .RpcRequest<AuthorizeUser, UserAuthorizationResult>(new AuthorizeUser(username, password), CancellationToken.None)
                   .ConfigureAwait(false);
            }

            if (authorizationResult.Token.IsNullOrEmpty())
            {
                return AuthenticateResult.Fail(authorizationResult.Details);
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}