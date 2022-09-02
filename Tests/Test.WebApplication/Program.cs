namespace SpaceEngineers.Core.Test.WebApplication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;
    using AuthEndpoint.Host;
    using Basics;
    using CrossCuttingConcerns.Extensions;
    using DataAccess.Orm.PostgreSql.Host;
    using GenericHost;
    using IntegrationTransport.WebHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Migrations;
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
            return BuildHost(args).RunAsync();
        }

        /// <summary>
        /// BuildHost
        /// </summary>
        /// <param name="args">args</param>
        /// <returns>IHost</returns>
        public static IHost BuildHost(string[] args)
        {
            var migrations = new[]
            {
                typeof(RecreatePostgreSqlDatabaseMigration),
                typeof(AddSeedDataMigration)
            };

            return Host
               .CreateDefaultBuilder(args)
               .UseIntegrationTransport(hostBuilder =>
                    context => new WebApplicationStartup(hostBuilder,
                        context.Configuration,
                        builder => builder
                           .WithRabbitMqIntegrationTransport(hostBuilder)
                           .WithWebApi(hostBuilder)
                           .BuildOptions(),
                        "Gateway"))
               .UseAuthEndpoint(builder => builder
                   .WithDataAccess(new PostgreSqlDatabaseProvider(),
                        options => options
                           .ExecuteMigrations())
                   .ModifyContainerOptions(options => options
                       .WithAdditionalOurTypes(migrations))
                   .BuildOptions())
               .BuildWebHost(GetFileSystemSettingsDirectory());
        }

        private static DirectoryInfo GetFileSystemSettingsDirectory()
        {
            return SolutionExtensions
               .SolutionFile()
               .Directory
               .EnsureNotNull("Solution directory wasn't found")
               .StepInto("Tests")
               .StepInto("Test.WebApplication")
               .StepInto("Settings");
        }

        private static string GetEndpointInstanceName(string endpointLogicalName)
        {
            var endpointSettingsFilePath = SolutionExtensions
               .SolutionFile()
               .Directory
               .EnsureNotNull("Solution directory wasn't found")
               .StepInto("Tests")
               .StepInto("Test.WebApplication")
               .StepInto("Settings")
               .StepInto(endpointLogicalName)
               .GetFile("appsettings", ".json")
               .FullName;

            var endpointConfiguration = new ConfigurationBuilder()
               .AddJsonFile(endpointSettingsFilePath)
               .Build();

            return endpointConfiguration.GetRequiredValue<string>("InstanceName")
                ?? throw new InvalidOperationException($"Unable to find 'InstanceName' for {endpointLogicalName} in scoped settings");
        }
    }
}