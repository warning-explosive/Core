namespace SpaceEngineers.Core.GenericEndpoint.Conventions
{
    using SettingsManager;
    using SettingsManager.Abstractions;

    /// <summary>
    /// Queue conventions
    /// </summary>
    public class QueueConventions : IYamlSettings
    {
        /// <summary>
        /// Error queue name
        /// </summary>
        public string ErrorQueueName { get; set; }

        /// <summary>
        /// Audit queue name
        /// </summary>
        public string AuditQueueName { get; set; }

        /// <summary>
        /// Monitoring queue name
        /// Equals to Monitoring service instance name
        /// </summary>
        public string MonitoringQueueName { get; set; }

        /// <summary>
        /// Service control queue
        /// Equals to ServiceControl service instance name
        /// </summary>
        public string ServiceControlQueue { get; set; }
    }
}