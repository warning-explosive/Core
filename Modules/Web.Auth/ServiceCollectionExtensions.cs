namespace SpaceEngineers.Core.Web.Auth
{
    using System.Net.Http.Headers;
    using Basics;
    using JwtAuthentication;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// ServiceCollectionExtensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private const string CompositeAuthenticationScheme = "Composite";

        /// <summary>
        /// Adds authentication and authorization services into core container
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        public static void AddAuth(this IServiceCollection serviceCollection)
        {
            var authenticationConfiguration = HostExtensions
               .GetAuthEndpointConfiguration()
               .GetJwtAuthenticationConfiguration();

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
                    .AddRequirements(new CustomAuthorizationRequirement())
                    .Build();
            });

            serviceCollection.AddSingleton<IAuthorizationHandler, CustomAuthorizationHandler>();
        }

        private static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder)
        {
            return builder.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicDefaults.AuthenticationScheme, null);
        }
    }
}