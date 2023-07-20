namespace SpaceEngineers.Core.GenericEndpoint.Authorization.Web.Handlers
{
    using System;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AuthEndpoint.Contract;
    using Basics;
    using CompositionRoot;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    internal class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public BasicAuthenticationHandler(
            IDependencyContainer dependencyContainer,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _dependencyContainer = dependencyContainer;
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

            UserAuthenticationResult userAuthenticationResult;

            await using (_dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                userAuthenticationResult = await _dependencyContainer
                    .Resolve<IIntegrationContext>()
                    .RpcRequest<AuthenticateUser, UserAuthenticationResult>(new AuthenticateUser(username, password), CancellationToken.None)
                    .ConfigureAwait(false);
            }

            if (userAuthenticationResult.Token.IsNullOrEmpty())
            {
                return AuthenticateResult.Fail("Wrong username or password");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Authentication, userAuthenticationResult.Token)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}