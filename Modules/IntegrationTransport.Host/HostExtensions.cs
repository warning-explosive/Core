namespace SpaceEngineers.Core.IntegrationTransport.Host
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Api.Enumerations;
    using BackgroundWorkers;
    using Basics;
    using Basics.Primitives;
    using Builder;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Extensions;
    using GenericHost;
    using GenericHost.Api;
    using GenericHost.Api.Abstractions;
    using GenericHost.Internals;
    using ManualRegistrations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        private const string RequireUseTransportCall = ".UseIntegrationTransport() should be called before any endpoint declarations. {0}";
        private const string RequireTransportDependencyContainer = "Unable to resolve transport IDependencyContainer.";

        /// <summary>
        /// Gets endpoint dependency container
        /// </summary>
        /// <param name="host">IHost</param>
        /// <returns>IDependencyContainer</returns>
        public static IDependencyContainer GetTransportDependencyContainer(this IHost host)
        {
            return GetTransportEndpointDependencyContainer(host.Services);
        }

        /// <summary>
        /// Waits until transport is not in running state
        /// </summary>
        /// <param name="host">IHost</param>
        /// <param name="log">Logging action</param>
        /// <returns>Ongoing operation</returns>
        public static async Task WaitUntilTransportIsNotRunning(this IHost host, Action<string> log)
        {
            var tcs = new TaskCompletionSource<object>();
            var subscription = MakeSubscription(tcs, log);

            var integrationTransport = host
                .GetTransportDependencyContainer()
                .Resolve<IIntegrationTransport>();

            using (Disposable.Create((integrationTransport, subscription), Subscribe, Unsubscribe))
            {
                log("Wait until transport is not started");
                await tcs.Task.ConfigureAwait(false);
            }

            static EventHandler<IntegrationTransportStatusChangedEventArgs> MakeSubscription(TaskCompletionSource<object> tcs, Action<string> log)
            {
                return (_, args) =>
                {
                    if (args.CurrentStatus == EnIntegrationTransportStatus.Running)
                    {
                        tcs.TrySetResult(default!);
                    }
                };
            }

            static void Subscribe((IIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (integrationTransport, subscription) = state;
                integrationTransport.StatusChanged += subscription;
            }

            static void Unsubscribe((IIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (integrationTransport, subscription) = state;
                integrationTransport.StatusChanged -= subscription;
            }
        }

        /// <summary>
        /// Use in-memory integration transport inside specified host
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="optionsFactory">Transport endpoint options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseIntegrationTransport(
            this IHostBuilder hostBuilder,
            Func<ITransportEndpointBuilder, TransportEndpointOptions> optionsFactory)
        {
            hostBuilder.CheckMultipleCalls(nameof(UseIntegrationTransport));

            return hostBuilder.ConfigureServices((_, serviceCollection) => InitializeIntegrationTransport(hostBuilder, optionsFactory)(serviceCollection));
        }

        internal static Action<IServiceCollection> InitializeIntegrationTransport(
            IHostBuilder hostBuilder,
            Func<ITransportEndpointBuilder, TransportEndpointOptions> optionsFactory)
        {
            var builder = ConfigureBuilder(hostBuilder);

            var options = optionsFactory(builder);

            var dependencyContainer = BuildDependencyContainer(options);

            hostBuilder.Properties[nameof(IIntegrationTransport)] = new IntegrationTransportInjectionManualRegistration(dependencyContainer);

            return serviceCollection =>
            {
                serviceCollection.AddSingleton<IDependencyContainer>(dependencyContainer);

                foreach (var producer in builder.StartupActions)
                {
                    serviceCollection.AddSingleton<IHostStartupAction>(producer(dependencyContainer));
                }

                foreach (var producer in builder.BackgroundWorkers)
                {
                    serviceCollection.AddSingleton<IHostBackgroundWorker>(producer(dependencyContainer));
                }
            };
        }

        internal static IDependencyContainer GetTransportEndpointDependencyContainer(
            this IServiceProvider serviceProvider)
        {
            return serviceProvider
               .GetServices<IDependencyContainer>()
               .SingleOrDefault(IsTransportContainer)
               .EnsureNotNull(RequireUseTransportCall.Format(RequireTransportDependencyContainer));

            static bool IsTransportContainer(IDependencyContainer container)
            {
                return ExecutionExtensions
                   .Try(container, IsTransportContainerUnsafe)
                   .Catch<Exception>()
                   .Invoke(_ => false);
            }

            static bool IsTransportContainerUnsafe(IDependencyContainer container)
            {
                return container
                   .Resolve<EndpointIdentity>()
                   .LogicalName
                   .Equals(TransportEndpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase);
            }
        }

        private static ITransportEndpointBuilder ConfigureBuilder(
            IHostBuilder hostBuilder)
        {
            var messagingAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.GenericEndpoint),
                    nameof(Core.GenericEndpoint.Messaging)));

            var crossCuttingConcernsAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.CrossCuttingConcerns)));

            var endpointIdentity = new EndpointIdentity(TransportEndpointIdentity.LogicalName, Guid.NewGuid());

            var frameworkDependenciesProvider = hostBuilder.GetFrameworkDependenciesProvider();

            var integrationMessageTypes = AssembliesExtensions
               .AllOurAssembliesFromCurrentDomain()
               .SelectMany(assembly => assembly.GetTypes())
               .Where(type => typeof(IIntegrationMessage).IsAssignableFrom(type)
                           && !type.IsMessageContractAbstraction())
               .ToArray();

            return new TransportEndpointBuilder(endpointIdentity)
               .WithBackgroundWorker(dependencyContainer => new IntegrationTransportHostBackgroundWorker(dependencyContainer))
               .WithEndpointPluginAssemblies(messagingAssembly, crossCuttingConcernsAssembly)
               .ModifyContainerOptions(options => options
                   .WithAdditionalOurTypes(integrationMessageTypes)
                   .WithManualRegistrations(new TransportEndpointIdentityManualRegistration(endpointIdentity))
                   .WithManualRegistrations(new LoggerFactoryManualRegistration(endpointIdentity, frameworkDependenciesProvider))
                   .WithManualRegistrations(new ConfigurationProviderManualRegistration())
                   .WithManualVerification(true));
        }

        private static IDependencyContainer BuildDependencyContainer(TransportEndpointOptions options)
        {
            return DependencyContainer.CreateBoundedAbove(
                options.ContainerOptions,
                options.AboveAssemblies.ToArray());
        }
    }
}