namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Settings
{
    using System.Diagnostics.CodeAnalysis;
    using CrossCuttingConcerns.Settings;

    /// <summary>
    /// RabbitMqSettings
    /// </summary>
    public class RabbitMqSettings : ISettings
    {
        /// <summary> .cctor </summary>
        public RabbitMqSettings()
        {
            Hosts = new[] { "127.0.0.1" };
            Port = 5672;
            HttpApiPort = 15672;
            User = "guest";
            Password = "guest";
            VirtualHost = "/";
            ApplicationName = "Andromeda";
            ConsumerPrefetchCount = 100;
            QueueMaxLengthBytes = 1024 * 1024;
            ConsumerPriority = 0;
        }

        /// <summary>
        /// Hosts
        /// </summary>
        [SuppressMessage("Analysis", "CA1819", Justification = "Settings")]
        public string[] Hosts { get; init; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; init; }

        /// <summary>
        /// Http api port
        /// </summary>
        public int HttpApiPort { get; init; }

        /// <summary>
        /// User
        /// </summary>
        public string User { get; init; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; init; }

        /// <summary>
        /// Virtual host
        /// </summary>
        public string VirtualHost { get; init; }

        /// <summary>
        /// Application name
        /// </summary>
        public string ApplicationName { get; init; }

        /// <summary>
        /// Channel prefetch count
        /// Prefetch limit -> Consumer utilisation
        /// 1 -> 14%
        /// 3 -> 25%
        /// 10 -> 46%
        /// 30 -> 70%
        /// 1000 -> 74%
        /// </summary>
        public ushort ConsumerPrefetchCount { get; init; }

        /// <summary>
        /// Queue max length bytes
        /// </summary>
        public int QueueMaxLengthBytes { get; init; }

        /// <summary>
        /// Consumer priority
        /// </summary>
        public ushort ConsumerPriority { get; init; }
    }
}