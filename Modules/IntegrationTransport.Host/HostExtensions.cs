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
    using GenericHost.Api;
    using GenericHost.Api.Abstractions;
    using ManualRegistrations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        private const string RequireUseTransportCall = ".UseIntegrationTransport() should be called before any endpoint declarations";

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

            return hostBuilder.ConfigureServices((_, serviceCollection) => InitializeIntegrationTransport(optionsFactory)(serviceCollection));
        }

        internal static Action<IServiceCollection> InitializeIntegrationTransport(
            Func<ITransportEndpointBuilder, TransportEndpointOptions> optionsFactory)
        {
            return serviceCollection =>
            {
                var messagingAssembly = AssembliesExtensions.FindRequiredAssembly(
                    AssembliesExtensions.BuildName(
                        nameof(SpaceEngineers),
                        nameof(Core),
                        nameof(Core.GenericEndpoint),
                        nameof(Core.GenericEndpoint.Messaging)));

                var builder = new TransportEndpointBuilder()
                    .WithBackgroundWorker(dependencyContainer => new IntegrationTransportHostBackgroundWorker(dependencyContainer))
                    .WithEndpointPluginAssemblies(messagingAssembly);

                var transportEndpointOptions = optionsFactory(builder);

                serviceCollection.AddSingleton<IDependencyContainer>(serviceProvider => BuildTransportContainer(ConfigureTransportEndpointOptions(serviceProvider, transportEndpointOptions)));

                foreach (var producer in builder.StartupActions)
                {
                    serviceCollection.AddSingleton<IHostStartupAction>(serviceProvider => BuildEndpointStartupAction(serviceProvider, producer));
                }

                foreach (var producer in builder.BackgroundWorkers)
                {
                    serviceCollection.AddSingleton<IHostBackgroundWorker>(serviceProvider => BuildEndpointBackgroundWorker(serviceProvider, producer));
                }
            };
        }

        internal static IDependencyContainer GetTransportEndpointDependencyContainer(this IServiceProvider serviceProvider)
        {
            return serviceProvider
                       .GetServices<IDependencyContainer>()
                       .SingleOrDefault(IsTransportContainer)
                   ?? throw new InvalidOperationException(RequireUseTransportCall);

            static bool IsTransportContainer(IDependencyContainer dependencyContainer)
            {
                return ExecutionExtensions
                    .Try(dependencyContainer, IsTransportContainerUnsafe)
                    .Catch<Exception>()
                    .Invoke(_ => false);
            }

            static bool IsTransportContainerUnsafe(IDependencyContainer dependencyContainer)
            {
                var endpointIdentity = dependencyContainer.Resolve<EndpointIdentity>();

                return endpointIdentity.LogicalName.Equals(nameof(IntegrationTransport), StringComparison.OrdinalIgnoreCase);
            }
        }

        private static TransportEndpointOptions ConfigureTransportEndpointOptions(
            IServiceProvider serviceProvider,
            TransportEndpointOptions transportEndpointOptions)
        {
            var endpointIdentity = new EndpointIdentity(nameof(IntegrationTransport), Guid.NewGuid());

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            var containerOptions = transportEndpointOptions
                .ContainerOptions
                .WithManualRegistrations(new TransportEndpointIdentityManualRegistration(endpointIdentity))
                .WithManualRegistrations(new LoggerFactoryManualRegistration(endpointIdentity, loggerFactory));

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

        private static IHostStartupAction BuildEndpointStartupAction(
            IServiceProvider serviceProvider,
            Func<IDependencyContainer, IHostStartupAction> producer)
        {
            var dependencyContainer = GetTransportEndpointDependencyContainer(serviceProvider);
            return producer(dependencyContainer);
        }

        private static IHostBackgroundWorker BuildEndpointBackgroundWorker(
            IServiceProvider serviceProvider,
            Func<IDependencyContainer, IHostBackgroundWorker> producer)
        {
            var dependencyContainer = GetTransportEndpointDependencyContainer(serviceProvider);
            return producer(dependencyContainer);
        }
    }
}