namespace SpaceEngineers.Core.GenericEndpoint.Web.Api.Host
{
    using System.Net.Http.Headers;
    using Authorization.Web.Api.Handlers;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Primitives;
    using Basics;
    using SpaceEngineers.Core.GenericEndpoint.Authorization.Host;

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
        /// <param name="configuration">IConfiguration</param>
        public static void AddAuth(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
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
                    var authenticationConfiguration = configuration.GetJwtAuthenticationConfiguration();

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