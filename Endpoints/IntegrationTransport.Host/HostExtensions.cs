namespace SpaceEngineers.Core.IntegrationTransport.Host
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Api.Enumerations;
    using Basics;
    using Basics.Primitives;
    using Builder;
    using CompositionRoot;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Extensions;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.Builder;
    using GenericEndpoint.Host.Overrides;
    using GenericEndpoint.Host.Registrations;
    using GenericHost;
    using GenericHost.Api;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Overrides;
    using Registrations;

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
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public static async Task WaitUntilTransportIsNotRunning(this IHost host, CancellationToken token)
        {
            using (var tcs = new TaskCancellationCompletionSource<object?>(token))
            {
                var subscription = MakeSubscription(tcs);

                var integrationTransport = host
                   .GetTransportDependencyContainer()
                   .Resolve<IExecutableIntegrationTransport>();

                using (Disposable.Create((integrationTransport, subscription), Subscribe, Unsubscribe))
                {
                    await tcs.Task.ConfigureAwait(false);
                }
            }

            static EventHandler<IntegrationTransportStatusChangedEventArgs> MakeSubscription(
                TaskCompletionSource<object?> tcs)
            {
                return (_, args) =>
                {
                    if (args.CurrentStatus == EnIntegrationTransportStatus.Running)
                    {
                        tcs.TrySetResult(default!);
                    }
                };
            }

            static void Subscribe((IExecutableIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
            {
                var (integrationTransport, subscription) = state;
                integrationTransport.StatusChanged += subscription;
            }

            static void Unsubscribe((IExecutableIntegrationTransport, EventHandler<IntegrationTransportStatusChangedEventArgs>) state)
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
        /// <param name="logicalName">Logical name</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseIntegrationTransport(
            this IHostBuilder hostBuilder,
            Func<ITransportEndpointBuilder, EndpointOptions> optionsFactory,
            string? logicalName = null)
        {
            return hostBuilder.ConfigureServices((_, serviceCollection) => InitializeIntegrationTransport(hostBuilder, optionsFactory, logicalName)(serviceCollection));
        }

        internal static Action<IServiceCollection> InitializeIntegrationTransport(
            IHostBuilder hostBuilder,
            Func<ITransportEndpointBuilder, EndpointOptions> optionsFactory,
            string? logicalName = null)
        {
            hostBuilder.CheckMultipleCalls(nameof(UseIntegrationTransport));

            var endpointIdentity = new EndpointIdentity(
                logicalName.IsNullOrEmpty() ? TransportEndpointIdentity.LogicalName : $"{TransportEndpointIdentity.LogicalName}{logicalName}",
                Guid.NewGuid().ToString());

            hostBuilder.ApplyOptions(endpointIdentity);

            var builder = ConfigureBuilder(hostBuilder, endpointIdentity);

            var options = optionsFactory(builder);

            var dependencyContainer = BuildDependencyContainer(options);

            hostBuilder.Properties[nameof(IIntegrationTransport)] = new IntegrationTransportInjectionManualRegistration(dependencyContainer);

            return serviceCollection =>
            {
                serviceCollection.AddSingleton<IDependencyContainer>(dependencyContainer);
                serviceCollection.AddSingleton<ITransportDependencyContainer>(new TransportDependencyContainer(dependencyContainer));
            };
        }

        internal static IDependencyContainer GetTransportEndpointDependencyContainer(
            this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<ITransportDependencyContainer>()?.DependencyContainer
                ?? throw new InvalidOperationException(RequireUseTransportCall.Format(RequireTransportDependencyContainer));
        }

        private static IDependencyContainer BuildDependencyContainer(EndpointOptions options)
        {
            return DependencyContainer.CreateBoundedAbove(
                options.ContainerOptions,
                options.AboveAssemblies.ToArray());
        }

        private static ITransportEndpointBuilder ConfigureBuilder(
            IHostBuilder hostBuilder,
            EndpointIdentity endpointIdentity)
        {
            var crossCuttingConcernsAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.CrossCuttingConcerns)));

            var frameworkDependenciesProvider = hostBuilder.GetFrameworkDependenciesProvider();

            var integrationMessageTypes = AssembliesExtensions
               .AllOurAssembliesFromCurrentDomain()
               .SelectMany(assembly => assembly.GetTypes())
               .Where(type => typeof(IIntegrationMessage).IsAssignableFrom(type)
                           && !type.IsMessageContractAbstraction())
               .ToArray();

            var aggregates = TypeExtensions.TryFindType("SpaceEngineers.Core.GenericDomain.Api SpaceEngineers.Core.GenericDomain.Api.Abstractions.IAggregate", out var aggregateType)
                    ? AssembliesExtensions
                       .AllOurAssembliesFromCurrentDomain()
                       .SelectMany(assembly => assembly.GetTypes())
                       .Where(type => aggregateType.IsAssignableFrom(type))
                       .ToArray()
                    : Array.Empty<Type>();

            var domainEvents = TypeExtensions.TryFindType("SpaceEngineers.Core.GenericDomain.Api SpaceEngineers.Core.GenericDomain.Api.Abstractions.IDomainEvent", out var domainEventType)
                    ? AssembliesExtensions
                       .AllOurAssembliesFromCurrentDomain()
                       .SelectMany(assembly => assembly.GetTypes())
                       .Where(type => domainEventType.IsAssignableFrom(type))
                       .ToArray()
                    : Array.Empty<Type>();

            return (ITransportEndpointBuilder)new TransportEndpointBuilder(endpointIdentity)
               .WithEndpointPluginAssemblies(crossCuttingConcernsAssembly)
               .ModifyContainerOptions(options => options
                   .WithAdditionalOurTypes(integrationMessageTypes)
                   .WithAdditionalOurTypes(aggregates)
                   .WithAdditionalOurTypes(domainEvents)
                   .WithManualRegistrations(new GenericEndpointIdentityManualRegistration(endpointIdentity))
                   .WithManualRegistrations(new LoggerFactoryManualRegistration(endpointIdentity, frameworkDependenciesProvider))
                   .WithManualRegistrations(new HostStartupActionsRegistryManualRegistration(frameworkDependenciesProvider))
                   .WithManualRegistrations(new GenericEndpointHostStartupActionManualRegistration())
                   .WithManualRegistrations(new IntegrationTransportHostBackgroundWorkerManualRegistration())
                   .WithOverrides(new SettingsProviderOverride())
                   .WithOverrides(new IntegrationTransportOverride())
                   .WithManualVerification(true));
        }
    }
}