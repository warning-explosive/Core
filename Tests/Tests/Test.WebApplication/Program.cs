namespace SpaceEngineers.Core.Test.WebApplication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AuthEndpoint.Host;
    using Basics;
    using GenericEndpoint.Authorization.Host;
    using GenericEndpoint.Authorization.Web.Host;
    using GenericEndpoint.DataAccess.Sql.Postgres.Host;
    using GenericEndpoint.EventSourcing.Host;
    using GenericEndpoint.Host;
    using GenericEndpoint.Telemetry.Host;
    using GenericEndpoint.Web.Host;
    using GenericHost;
    using IntegrationTransport.Host;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Migrations;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Trace;
    using Registrations;
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
            return Host
                .CreateDefaultBuilder(args)

                /*
                 * logging
                 */

                .ConfigureLogging((context, builder) =>
                {
                    builder.ClearProviders();
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                    builder.AddJsonConsole(options =>
                        {
                            options.JsonWriterOptions = new JsonWriterOptions
                            {
                                Indented = true
                            };
                            options.IncludeScopes = false;
                        })
                        .SetMinimumLevel(LogLevel.Trace);
                })

                /*
                 * IntegrationTransport
                 */

                .UseRabbitMqIntegrationTransport(
                    IntegrationTransport.RabbitMQ.Identity.TransportIdentity(),
                    options => options.WithManualRegistrations(new PurgeRabbitMqQueuesManualRegistration()))

                /*
                 * AuthEndpoint
                 */

                .UseAuthEndpoint(builder => builder
                    .WithPostgreSqlDataAccess(options => options.ExecuteMigrations())
                    .WithSqlEventSourcing()
                    .WithOpenTelemetry()
                    .WithJwtAuthentication(builder.Context.Configuration)
                    .WithAuthorization()
                    .WithWebAuthorization()
                    .WithWebApi()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(
                            typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction),
                            typeof(AddSeedDataMigration)))
                    .BuildOptions())

                /*
                 * TestEndpoint
                 */

                .UseEndpoint(
                    Identity.EndpointIdentity,
                    builder => builder
                        .WithOpenTelemetry()
                        .WithJwtAuthentication(builder.Context.Configuration)
                        .WithAuthorization()
                        .WithWebAuthorization()
                        .WithWebApi()
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(typeof(TestController)))
                        .BuildOptions())

                /*
                 * WebApiGateway
                 */

                .UseWebApiGateway()

                /*
                 * Telemetry
                 */
                .UseOpenTelemetry(
                    tracerProviderBuilder => tracerProviderBuilder
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation(),
                    meterProviderBuilder => meterProviderBuilder
                        .AddRuntimeInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation())

                /*
                 * Building
                 */

                .UseEnvironment(Environments.Development)

                .BuildHost(settingsDirectory);
        }
    }
}