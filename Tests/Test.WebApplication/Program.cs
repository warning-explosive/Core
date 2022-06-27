namespace SpaceEngineers.Core.Test.WebApplication
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using AuthorizationEndpoint.Host;
    using Basics;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.PostgreSql.Host;
    using GenericHost;
    using IntegrationTransport.WebHost;
    using Microsoft.Extensions.Hosting;
    using TracingEndpoint.Host;
    using Web.Api.Host;

    internal static class Program
    {
        [SuppressMessage("Analysis", "CA1506", Justification = "web application composition root")]
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
                .UseIntegrationTransport(hostBuilder =>
                    context => new WebApplicationStartup(
                        hostBuilder,
                        context.Configuration,
                        builder => builder
                            .WithInMemoryIntegrationTransport(hostBuilder)
                            .WithWebApi()
                            .WithTracing()
                            .BuildOptions()))
                .UseAuthorizationEndpoint(0, builder => builder
                    .WithDataAccess(new PostgreSqlDatabaseProvider())
                    .WithTracing()
                    .BuildOptions())
                .UseTracingEndpoint(0, builder => builder
                    .WithDataAccess(new PostgreSqlDatabaseProvider())
                    .BuildOptions())
                .BuildWebHost()
                .RunAsync()
                .ConfigureAwait(false);
        }
    }
}