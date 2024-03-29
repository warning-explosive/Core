namespace SpaceEngineers.Core.GenericEndpoint.Web.Host.Auth
{
    using System;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Basics;
    using JwtAuthentication;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using SpaceEngineers.Core.AuthEndpoint.Contract;
    using SpaceEngineers.Core.GenericEndpoint.Authorization.Web;

    internal class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /*private readonly IDependencyContainer _dependencyContainer;*/
        private readonly ITokenProvider _tokenProvider;

        public BasicAuthenticationHandler(
            ITokenProvider tokenProvider,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
            _tokenProvider = tokenProvider;
            /*_dependencyContainer = dependencyContainer;*/
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

            var authenticationHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"].ToString());

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

            /*await using (_dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                // TODO: #205 - recode back
                userAuthenticationResult = await _dependencyContainer
                    .Resolve<IIntegrationContext>()
                    .RpcRequest<AuthenticateUser, UserAuthenticationResult>(new AuthenticateUser(username, password), CancellationToken.None)
                    .ConfigureAwait(false);
            }*/

            // TODO: #205 - recode back
            {
                await Task.CompletedTask.ConfigureAwait(false);

                var token = _tokenProvider.GenerateToken(username, new[] { "WebApiTest", "Authentication" }, TimeSpan.FromHours(1));

                userAuthenticationResult = new UserAuthenticationResult(username, token);
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