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
    using CompositionRoot.Api.Abstractions.Container;
    using GenericEndpoint.Contract;
    using GenericHost;
    using GenericHost.Api;
    using GenericHost.Api.Abstractions;
    using InMemory.ManualRegistrations;
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
                return (s, e) =>
                {
                    log($"{s.GetType().Name}: {e.PreviousStatus} -> {e.CurrentStatus}");

                    if (e.CurrentStatus == EnIntegrationTransportStatus.Running)
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
            var transportEndpointOptions = ConfigureTransportEndpointOptions(hostBuilder, optionsFactory, out var builder);

            var transportDependencyContainer = BuildTransportContainer(transportEndpointOptions);

            hostBuilder.Properties[nameof(IIntegrationTransport)] = new InMemoryIntegrationTransportInjectionManualRegistration(transportDependencyContainer);

            return serviceCollection =>
            {
                serviceCollection.AddSingleton<IDependencyContainer>(transportDependencyContainer);

                foreach (var producer in builder.StartupActions)
                {
                    serviceCollection.AddSingleton<IHostStartupAction>(producer(transportDependencyContainer));
                }

                foreach (var producer in builder.BackgroundWorkers)
                {
                    serviceCollection.AddSingleton<IHostBackgroundWorker>(producer(transportDependencyContainer));
                }
            };
        }

        internal static IDependencyContainer GetTransportEndpointDependencyContainer(
            this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<IDependencyContainer>()
                   ?? throw new InvalidOperationException(RequireUseTransportCall.Format(RequireTransportDependencyContainer));
        }

        private static TransportEndpointOptions ConfigureTransportEndpointOptions(
            IHostBuilder hostBuilder,
            Func<ITransportEndpointBuilder, TransportEndpointOptions> optionsFactory,
            out ITransportEndpointBuilder builder)
        {
            var messagingAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.GenericEndpoint),
                    nameof(Core.GenericEndpoint.Messaging)));

            builder = new TransportEndpointBuilder()
                .WithBackgroundWorker(dependencyContainer => new IntegrationTransportHostBackgroundWorker(dependencyContainer))
                .WithEndpointPluginAssemblies(messagingAssembly);

            var transportEndpointOptions = optionsFactory(builder);

            var endpointIdentity = new EndpointIdentity(nameof(IntegrationTransport), Guid.NewGuid());

            var frameworkDependenciesProvider = hostBuilder.GetFrameworkDependenciesProvider();

            var containerOptions = transportEndpointOptions
                .ContainerOptions
                .WithManualRegistrations(new TransportEndpointIdentityManualRegistration(endpointIdentity))
                .WithManualRegistrations(new LoggerFactoryManualRegistration(endpointIdentity, frameworkDependenciesProvider))
                .WithManualVerification(true);

            return transportEndpointOptions.WithContainerOptions(containerOptions);
        }

        private static IDependencyContainer BuildTransportContainer(
            TransportEndpointOptions transportEndpointOptions)
        {
            return DependencyContainer.CreateBoundedAbove(
                transportEndpointOptions.ContainerOptions,
                transportEndpointOptions.ContainerImplementationProducer(transportEndpointOptions.ContainerOptions),
                transportEndpointOptions.AboveAssemblies.ToArray());
        }
    }
}