namespace SpaceEngineers.Core.Web.Auth
{
    using System.Net.Http.Headers;
    using Authentication;
    using AuthorizationEndpoint.Host;
    using Basics;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// Authentication and authorization extensions
    /// </summary>
    public static class AuthExtensions
    {
        private const string CompositeAuthenticationScheme = "Composite";

        /// <summary>
        /// Adds authentication and authorization services into core container
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        /// <param name="configuration">IConfiguration</param>
        public static void AddAuth(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            var authenticationConfiguration = configuration.GetAuthenticationConfiguration();

            serviceCollection
                .AddAuthentication(CompositeAuthenticationScheme)
                .AddPolicyScheme(CompositeAuthenticationScheme, CompositeAuthenticationScheme, options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var header = context.Request.Headers["Authorization"];

                        return !StringValues.IsNullOrEmpty(header)
                            ? AuthenticationHeaderValue
                                .Parse(header)
                                .Scheme
                                .StartFromCapitalLetter()
                            : BasicDefaults.AuthenticationScheme;
                    };
                })
                .AddBasic()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.ClaimsIssuer = authenticationConfiguration.Issuer;
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = authenticationConfiguration.TokenValidationParameters;
                });

            serviceCollection.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
        }

        private static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder)
        {
            return builder.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicDefaults.AuthenticationScheme, null);
        }
    }
}