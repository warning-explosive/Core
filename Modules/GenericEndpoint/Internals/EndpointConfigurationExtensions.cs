﻿namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System;
    using MongoDB.Driver;
    using NServiceBus;
    using Settings;

    internal static class EndpointConfigurationExtensions
    {
        internal static EndpointConfiguration NamedEndpointConfiguration(string endpointName)
        {
            var configuration = new EndpointConfiguration(endpointName);
            configuration.OverrideLocalAddress($"{endpointName}.Input");
            configuration.UniquelyIdentifyRunningInstance()
                         .UsingNames(endpointName, Environment.MachineName);

            return configuration;
        }

        internal static EndpointConfiguration ConfigureServiceQueues(this EndpointConfiguration configuration, QueueConventions conventions)
        {
            configuration.SendFailedMessagesTo(conventions.ErrorQueueName);
            configuration.AuditProcessedMessagesTo(conventions.AuditQueueName);

            var metrics = configuration.EnableMetrics();
            metrics.SendMetricDataToServiceControl(conventions.MonitoringQueueName, TimeSpan.FromSeconds(5));

            configuration.SendHeartbeatTo(conventions.ServiceControlQueueName, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30));

            return configuration;
        }

        internal static EndpointConfiguration ConfigureTransport(this EndpointConfiguration configuration,
                                                                 TransportSettings settings)
        {
            var transport = configuration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString(settings.RabbitMqConnectionString.ToString());
            transport.Transactions(TransportTransactionMode.ReceiveOnly);
            transport.UseConventionalRoutingTopology();
            transport.UsePublisherConfirms(true);

            configuration.EnableDurableMessages();
            transport.UseDurableExchangesAndQueues(true);

            transport.PrefetchCount(settings.PrefetchCount);
            transport.SetHeartbeatInterval(settings.HeartbeatInterval);
            transport.SetNetworkRecoveryInterval(settings.NetworkRecoveryInterval);
            transport.TimeToWaitBeforeTriggeringCircuitBreaker(settings.CircuitBreakerRecoveryInterval);

            return configuration;
        }

        internal static EndpointConfiguration ConfigurePersistence(this EndpointConfiguration configuration, PersistenceSettings settings)
        {
            var persistence = configuration.UsePersistence<MongoPersistence>();
            persistence.MongoClient(new MongoClient(settings.MongoClientSettings));
            persistence.UseTransactions(false);

            return configuration;
        }

        internal static EndpointConfiguration ConfigureSerializer(this EndpointConfiguration configuration)
        {
            configuration.UseSerialization<NewtonsoftSerializer>();

            return configuration;
        }

        internal static EndpointConfiguration ConfigureDependencyInjection(this EndpointConfiguration configuration)
        {
            configuration.EnableInstallers();
            configuration.EnableUniformSession();

            return configuration;
        }

        internal static EndpointConfiguration Configure(this EndpointConfiguration configuration,
                                                        Action<EndpointConfiguration>? configure)
        {
            configure?.Invoke(configuration);

            return configuration;
        }
    }
}