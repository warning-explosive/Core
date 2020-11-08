namespace SpaceEngineers.Core.GenericEndpoint
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Internals;
    using MongoDB.Driver;
    using NewtonSoft.Json.Abstractions;
    using NServiceBus;
    using NServiceBus.ObjectBuilder;
    using NServiceBus.ObjectBuilder.Common;
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

        /// <summary> Endpoint configuration </summary>
        /// <param name="endpointName">Endpoint logical name</param>
        /// <returns>Configuration</returns>
        public static EndpointConfiguration Configuration(string endpointName)
        {
            var settingsContainer = SettingsContainer();
            var queueConventions = settingsContainer.GetSetting<QueueConventions>().Result;
            var transportSettings = settingsContainer.GetSetting<TransportSettings>().Result;
            var persistenceSettings = settingsContainer.GetSetting<PersistenceSettings>().Result;

            return GetEndpointConfiguration(endpointName, queueConventions, transportSettings, persistenceSettings);
        }

        /// <summary> Run endpoint </summary>
        /// <param name="endpointName">Endpoint name</param>
        /// <returns>Async cleanup</returns>
        public static async Task<IEndpoint> Run(string endpointName) // TODO: Remove
        {
            var dependencyContainer = DependencyContainer.Create(new DependencyContainerOptions());
            var builder = Builder();
            var compositeBuilder = new CompositeBuilder((IBuilder)builder, (IConfigureComponents)builder, dependencyContainer);

            var endpointConfiguration = Configuration(endpointName);
            var endpoint = EndpointWithExternallyManagedContainer.Create(endpointConfiguration, compositeBuilder);
            var endpointInstance = await endpoint.Start(compositeBuilder).ConfigureAwait(false);
            Console.WriteLine("Instance successfully started. Press any key to stop them.");

            return new Endpoint(dependencyContainer, CleanUp);

            async Task CleanUp()
            {
                await endpointInstance.Stop().ConfigureAwait(false);
                compositeBuilder.Dispose();
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
                            .ConfigureCustomFeatures();
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

        private static object Builder()
        {
            var assembly = typeof(NServiceBus.Endpoint).Assembly;
            var container = (IContainer)Activator.CreateInstance(assembly.GetType("NServiceBus.LightInjectObjectBuilder"));
            return Activator.CreateInstance(assembly.GetType("NServiceBus.CommonObjectBuilder"), container);
        }
    }
}