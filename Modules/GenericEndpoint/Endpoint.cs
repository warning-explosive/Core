namespace SpaceEngineers.Core.GenericEndpoint
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Basics;
    using Internals;
    using MongoDB.Driver;
    using NewtonSoft.Json.Abstractions;
    using NServiceBus;
    using NServiceBus.Installation;
    using NServiceBus.MessageMutator;
    using NServiceBus.ObjectBuilder;
    using Settings;
    using SettingsManager.Abstractions;

    /// <summary>
    /// Configured endpoint instance
    /// </summary>
    public class Endpoint : IEndpoint
    {
        private readonly IDependencyContainer _container;

        private readonly Func<Task> _cleanup;

        /// <summary> .cctor </summary>
        /// <param name="container">IDependencyContainer</param>
        /// <param name="cleanup">Cleanup</param>
        private Endpoint(IDependencyContainer container, Func<Task> cleanup)
        {
            _container = container;
            _cleanup = cleanup;
        }

        /// <summary> Run endpoint </summary>
        /// <param name="endpointName">Endpoint name</param>
        /// <param name="configure">Additional endpoint configuration. You can override exact configs.</param>
        /// <returns>Async cleanup</returns>
        public static async Task<IEndpoint> Run(string endpointName, Action<EndpointConfiguration>? configure = null)
        {
            var settingsContainer = SettingsContainer();
            var queueConventions = await settingsContainer.GetSetting<QueueConventions>().ConfigureAwait(false);
            var transportSettings = await settingsContainer.GetSetting<TransportSettings>().ConfigureAwait(false);
            var persistenceSettings = await settingsContainer.GetSetting<PersistenceSettings>().ConfigureAwait(false);

            var configuration = GetEndpointConfiguration(endpointName, configure, queueConventions, transportSettings, persistenceSettings);

            var container = ExternallyManagedContainer(configuration, out var endpointInstance, out var resolver);

            var runningInstance = await endpointInstance.Start(resolver).ConfigureAwait(false);

            Console.WriteLine("Instance successfully started. Press any key to stop them.");

            return new Endpoint(container, CleanUp);

            async Task CleanUp()
            {
                await runningInstance.Stop().ConfigureAwait(false);
                resolver.Dispose();
                Console.WriteLine("Instance successfully stopped.");
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _cleanup.Invoke().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task Execute(Func<IDependencyContainer, Task> worker)
        {
            return worker.Invoke(_container);
        }

        private static EndpointConfiguration GetEndpointConfiguration(string endpointName,
                                                                      Action<EndpointConfiguration>? configure,
                                                                      QueueConventions queueConventions,
                                                                      TransportSettings transportSettings,
                                                                      PersistenceSettings persistenceSettings)
        {
            return Internals.EndpointConfigurationExtensions
                            .NamedEndpointConfiguration(endpointName)
                            .ConfigureServiceQueues(queueConventions)
                            .ConfigureTransport(transportSettings)
                            .ConfigurePersistence(persistenceSettings)
                            .ConfigureSerializer()
                            .ConfigureDependencyInjection()
                            .ConfigureCustomFeatures()
                            .Configure(configure);
        }

        private static IDependencyContainer SettingsContainer()
        {
            var trustedAssemblies
                = new[]
                  {
                      typeof(object).Assembly,               // mscorlib
                      typeof(IEnumerable).Assembly,          // System.Collections
                      typeof(IEnumerable<>).Assembly,        // System.Collections.Generic
                      typeof(ReadOnlyCollection<>).Assembly, // System.Collections.ObjectModel

                      typeof(ISettingsManager<>).Assembly,              // SpaceEngineers.Core.SettingsManager
                      typeof(IJsonSerializer).Assembly,                 // SpaceEngineers.Core.Newtonsoft.Json
                      typeof(MongoServerAddressJsonConverter).Assembly, // SpaceEngineers.Core.GenericEndpoint

                      typeof(MongoClientSettings).Assembly,                                       // MongoDB.Driver
                      typeof(MongoDB.Driver.Core.Configuration.CompressorConfiguration).Assembly, // MongoDB.Driver.Core
                  };

            return DependencyContainer.CreateBounded(trustedAssemblies, new DependencyContainerOptions());
        }

        private static IDependencyContainer ExternallyManagedContainer(EndpointConfiguration endpointConfiguration,
                                                                       out IStartableEndpointWithExternallyManagedContainer outEndpointInstance,
                                                                       out IBuilder outResolver)
        {
            IBuilder? resolver = null;
            IStartableEndpointWithExternallyManagedContainer? endpointInstance = null;

            var options = new DependencyContainerOptions
                          {
                              RegistrationCallback = registration =>
                                                     {
                                                         registration.RegisterCollection(Enumerable.Empty<INeedToInstallSomething>());
                                                         registration.RegisterCollection(Enumerable.Empty<IMutateOutgoingMessages>());
                                                         registration.RegisterCollection(Enumerable.Empty<IMutateOutgoingTransportMessages>());

                                                         resolver = new NServiceBusDependencyContainerResolutionAdapter(registration);
                                                         var registrations = new NServiceBusDependencyContainerRegistrationAdapter(registration, resolver);
                                                         endpointInstance = EndpointWithExternallyManagedContainer.Create(endpointConfiguration, registrations);
                                                     }
                          };

            var dependencyContainer = DependencyContainer.Create(options);

            outEndpointInstance = endpointInstance.EnsureNotNull("Endpoint instance must be created");
            outResolver = resolver.EnsureNotNull("Endpoint dependency resolver must be created");

            return dependencyContainer;
        }
    }
}