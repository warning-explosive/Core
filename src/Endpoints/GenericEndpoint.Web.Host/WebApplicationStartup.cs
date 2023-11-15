namespace SpaceEngineers.Core.GenericEndpoint.Web.Host
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contract;
    using GenericEndpoint.Host;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// WebApplicationStartup
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification ="web application composition root")]
    public class WebApplicationStartup : IStartup
    {
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
        }

        /// <inheritdoc />
        public void Configure(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            if (!environment.IsProduction())
            {
                applicationBuilder.UseDeveloperExceptionPage();

                applicationBuilder.UseSwagger(options =>
                {
                    options.RouteTemplate = "api/{documentname}/swagger.json";
                });

                applicationBuilder.UseSwaggerUI(options =>
                {
                    var endpointIdentities = applicationBuilder
                        .ApplicationServices
                        .GetServices<GenericEndpointDependencyContainer>()
                        .Select(wrapper => wrapper
                            .DependencyContainer
                            .Resolve<EndpointIdentity>());

                    foreach (var endpointIdentity in endpointIdentities)
                    {
                        options.SwaggerEndpoint($"{endpointIdentity.LogicalName}/swagger.json", $"{endpointIdentity.LogicalName}:{endpointIdentity.Version}");
                    }

                    options.RoutePrefix = "api";
                });
            }
            else
            {
                applicationBuilder.UseExceptionHandler("/Error");
                applicationBuilder.UseHsts();
            }

            applicationBuilder.UseStaticFiles();
            applicationBuilder.UseCookiePolicy();

            applicationBuilder.UseRouting();
            applicationBuilder.UseRequestLocalization();
            applicationBuilder.UseCors(options => options
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            applicationBuilder.UseAuthentication();
            applicationBuilder.UseAuthorization();

            applicationBuilder.UseResponseCompression();
            applicationBuilder.UseResponseCaching();

            applicationBuilder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}