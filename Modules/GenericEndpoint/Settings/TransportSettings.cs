namespace SpaceEngineers.Core.GenericEndpoint.Settings
{
    using System;
    using SettingsManager.Abstractions;

    /// <summary>
    /// Transport settings
    /// </summary>
    public class TransportSettings : IJsonSettings
    {
        /// <summary> .cctor </summary>
        /// <param name="rabbitMqConnectionString">RabbitMqConnectionString</param>
        public TransportSettings(RabbitMqConnectionString rabbitMqConnectionString)
        {
            RabbitMqConnectionString = rabbitMqConnectionString;
            PrefetchCount = 50;
            HeartbeatInterval = TimeSpan.FromSeconds(15);
            NetworkRecoveryInterval = TimeSpan.FromSeconds(30);
            CircuitBreakerRecoveryInterval = TimeSpan.FromMinutes(2);
        }

        /// <summary>
        /// RabbitMq connection string
        /// </summary>
        public RabbitMqConnectionString RabbitMqConnectionString { get; }

        /// <summary>
        /// Prefetch count
        /// Defines the max number of unacknowledged deliveries that are permitted on a channel
        /// Once the number reaches the configured count, RabbitMQ will stop delivering more messages on the channel unless at least one of the outstanding ones is acknowledged
        /// Default: 50
        /// </summary>
        public ushort PrefetchCount { get; set; }

        /// <summary>
        /// Heartbeat interval
        /// Controls how frequently AMQP heartbeat messages will be sent between the endpoint and the broker
        /// After this interval with no response RabbitMQ will drop the connection
        /// Default: 15 seconds
        /// </summary>
        public TimeSpan HeartbeatInterval { get; set; }

        /// <summary>
        /// Network recovery interval
        /// Controls the time to wait between attempts to reconnect to the broker if the connection is lost
        /// Default: 30 seconds
        /// </summary>
        public TimeSpan NetworkRecoveryInterval { get; set; }

        /// <summary>
        /// CircuitBreaker recovery interval
        /// Time to wait before triggering a circuit breaker that initiates the endpoint shutdown procedure when the message pump's connection to the broker is lost and cannot be recovered.
        /// Default: 2 minutes
        /// </summary>
        public TimeSpan CircuitBreakerRecoveryInterval { get; set; }
    }
}