namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Settings
{
    using CrossCuttingConcerns.Settings;

    /// <summary>
    /// RabbitMqSettings
    /// </summary>
    public class RabbitMqSettings : ISettings
    {
        /// <summary> .cctor </summary>
        public RabbitMqSettings()
        {
            Host = "127.0.0.1";
            Port = 5672;
            HttpApiPort = 15672;
            User = "guest";
            Password = "guest";
            VirtualHost = "/";
            ApplicationName = "Andromeda";
            ChannelPrefetchCount = 100;
            QueueMaxLengthBytes = 1024 * 1024;
            ConsumerPriority = 0;
        }

        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Http api port
        /// </summary>
        public int HttpApiPort { get; set; }

        /// <summary>
        /// User
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Virtual host
        /// </summary>
        public string VirtualHost { get; set; }

        /// <summary>
        /// Application name
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Channel prefetch count
        /// Prefetch limit -> Consumer utilisation
        /// 1 -> 14%
        /// 3 -> 25%
        /// 10 -> 46%
        /// 30 -> 70%
        /// 1000 -> 74%
        /// </summary>
        public ushort ChannelPrefetchCount { get; set; }

        /// <summary>
        /// Queue max length bytes
        /// </summary>
        public int QueueMaxLengthBytes { get; set; }

        /// <summary>
        /// Consumer priority
        /// </summary>
        public ushort ConsumerPriority { get; set; }
    }
}