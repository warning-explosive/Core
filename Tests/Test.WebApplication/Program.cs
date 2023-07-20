namespace SpaceEngineers.Core.Test.WebApplication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AuthEndpoint.Host;
    using Basics;
    using GenericEndpoint.DataAccess.Sql.Postgres.Host;
    using GenericEndpoint.EventSourcing.Host;
    using GenericEndpoint.Telemetry.Host;
    using GenericEndpoint.Web.Host;
    using GenericHost;
    using IntegrationTransport.Host;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Migrations;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Trace;
    using StartupActions;

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
            var projectFileDirectory = SolutionExtensions.ProjectFile().Directory
                                       ?? throw new InvalidOperationException("Project directory wasn't found");

            var settingsDirectory = projectFileDirectory.StepInto("Settings");

            return BuildHost(settingsDirectory, args).RunAsync();
        }

        /// <summary>
        /// BuildHost
        /// </summary>
        /// <param name="settingsDirectory">Settings directory</param>
        /// <param name="args">args</param>
        /// <returns>IHost</returns>
        [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
        public static IHost BuildHost(DirectoryInfo settingsDirectory, string[] args)
        {
            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction)
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
                .ConfigureLogging(context => context
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Trace))
                .UseIntegrationTransport(hostBuilder =>
                    context => new WebApplicationStartup(
                        context,
                        hostBuilder,
                        context.Configuration,
                        builder => builder
                            .WithRabbitMqIntegrationTransport()
                            .WithWebApi(hostBuilder, context.Configuration)
                            .WithOpenTelemetry(
                                tracerProviderBuilder => tracerProviderBuilder
                                    .AddAspNetCoreInstrumentation()
                                    .AddHttpClientInstrumentation(),
                                meterProviderBuilder => meterProviderBuilder
                                    .AddRuntimeInstrumentation()
                                    .AddAspNetCoreInstrumentation()
                                    .AddHttpClientInstrumentation())
                            .ModifyContainerOptions(options => options
                                .WithManualRegistrations(new PurgeRabbitMqQueuesManualRegistration()))
                            .BuildOptions(),
                        "TransportEndpointGateway"))
                .UseOpenTelemetryLogger(AuthEndpoint.Contract.Identity.EndpointIdentity)
                .UseAuthEndpoint(builder => builder
                    .WithPostgreSqlDataAccess(options => options
                        .ExecuteMigrations())
                    .WithSqlEventSourcing()
                    .WithOpenTelemetry()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions())
                .BuildWebHost(settingsDirectory);
        }
    }
}