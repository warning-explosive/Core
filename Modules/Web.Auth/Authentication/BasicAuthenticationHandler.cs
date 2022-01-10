namespace SpaceEngineers.Core.Web.Auth.Authentication
{
    using System;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading;
    using System.Threading.Tasks;
    using AuthorizationEndpoint.Contract.Messages;
    using CompositionRoot.Api.Abstractions.Container;
    using IntegrationTransport.Api.Abstractions;
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
                return AuthenticateResult.Fail("Missing authentication header");
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

            var credentialBytes = Convert.FromBase64String(authenticationHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            var username = credentials[0];
            var password = credentials[1];

            var authorizationResult = await _dependencyContainer
                .Resolve<IIntegrationContext>()
                .RpcRequest<AuthorizeUser, UserAuthorizationResult>(new AuthorizeUser(username, password), CancellationToken.None)
                .ConfigureAwait(false);

            if (authorizationResult.Result)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, username)
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }

            return AuthenticateResult.Fail(authorizationResult.Details);
        }
    }
}