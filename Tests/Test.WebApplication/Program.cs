namespace SpaceEngineers.Core.WebApplication
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using AuthorizationEndpoint.Contract;
    using AuthorizationEndpoint.Host;
    using Basics;
    using CompositionRoot.SimpleInjector;
    using CrossCuttingConcerns.Api.Extensions;
    using DataAccess.Orm.PostgreSql;
    using GenericEndpoint.Contract;
    using GenericHost;
    using IntegrationTransport.WebHost;
    using Microsoft.Extensions.Hosting;
    using TracingEndpoint.Contract;
    using TracingEndpoint.Host;

    internal static class Program
    {
        [SuppressMessage("Analysis", "CA1506", Justification = "composition root")]
        public static async Task Main(string[] args)
        {
            SolutionExtensions
                .ProjectFile()
                .Directory
                .EnsureNotNull("Project directory not found")
                .StepInto("Settings")
                .SetupFileSystemSettingsDirectory();

            await Host
                .CreateDefaultBuilder(args)
                .UseIntegrationTransport(context => new WebApplicationStartup(
                    context.Configuration,
                    builder => builder
                        .WithContainer(options => options.UseSimpleInjector())
                        .WithInMemoryIntegrationTransport()
                        .WithDefaultCrossCuttingConcerns()
                        .WithTracing()
                        .BuildOptions()))
                .UseAuthorizationEndpoint(builder => builder
                    .WithContainer(options => options.UseSimpleInjector())
                    .WithDefaultCrossCuttingConcerns()
                    .WithDataAccess(new PostgreSqlDatabaseProvider())
                    .WithTracing()
                    .BuildOptions(new EndpointIdentity(AuthorizationEndpointIdentity.LogicalName, 0)))
                .UseTracingEndpoint(builder => builder
                    .WithContainer(options => options.UseSimpleInjector())
                    .WithDefaultCrossCuttingConcerns()
                    .WithDataAccess(new PostgreSqlDatabaseProvider())
                    .BuildOptions(new EndpointIdentity(TracingEndpointIdentity.LogicalName, 0)))
                .BuildHost()
                .RunAsync()
                .ConfigureAwait(false);
        }
    }
}