namespace SpaceEngineers.Core.Test.WebApplication
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AuthEndpoint.Host;
    using Basics;
    using GenericEndpoint.DataAccess.Host;
    using GenericEndpoint.EventSourcing.Host;
    using GenericHost;
    using IntegrationTransport.WebHost;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Migrations;
    using Registrations;
    using StartupActions;
    using Web.Api.Host;

    /// <summary>
    /// Program
    /// </summary>
    public static class Program
    {
        /// <summary> Main </summary>
        /// <param name="args">args</param>
        /// <returns>Ongoing operation</returns>
        [SuppressMessage("Analysis", "CA1506", Justification = "web application composition root")]
        public static Task Main(string[] args)
        {
            var settingsDirectory = SolutionExtensions
                .ProjectFile()
                .Directory
                .EnsureNotNull("Project directory wasn't found")
                .StepInto("Settings");

            return BuildHost(settingsDirectory, args).RunAsync();
        }

        /// <summary>
        /// BuildHost
        /// </summary>
        /// <param name="settingsDirectory">Settings directory</param>
        /// <param name="args">args</param>
        /// <returns>IHost</returns>
        public static IHost BuildHost(DirectoryInfo settingsDirectory, string[] args)
        {
            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostStartupAction)
            };

            var migrations = new[]
            {
                typeof(AddSeedDataMigration)
            };

            var additionalOurTypes = startupActions
                .Concat(migrations)
                .ToArray();

            return Host
               .CreateDefaultBuilder(args)
               .ConfigureLogging(context => context.AddConsole().SetMinimumLevel(LogLevel.Trace))
               .UseIntegrationTransport(hostBuilder =>
                    context => new WebApplicationStartup(
                        context,
                        hostBuilder,
                        context.Configuration,
                        builder => builder
                           .WithRabbitMqIntegrationTransport(hostBuilder)
                           .WithWebApi(hostBuilder, context.Configuration)
                           .ModifyContainerOptions(options => options
                               .WithManualRegistrations(new PurgeRabbitMqQueuesManualRegistration()))
                           .BuildOptions(),
                        "TransportEndpointGateway"))
               .UseAuthEndpoint(builder => builder
                   .WithPostgreSqlDataAccess(options => options
                       .ExecuteMigrations())
                   .WithSqlEventSourcing()
                   .ModifyContainerOptions(options => options
                       .WithAdditionalOurTypes(additionalOurTypes))
                   .BuildOptions())
               .BuildWebHost(settingsDirectory);
        }
    }
}