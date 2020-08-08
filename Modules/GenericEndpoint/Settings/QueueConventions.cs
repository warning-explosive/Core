namespace SpaceEngineers.Core.GenericEndpoint.Settings
{
    using SettingsManager.Abstractions;

    /// <summary>
    /// Queue conventions
    /// </summary>
    public class QueueConventions : IJsonSettings
    {
        /// <summary> .cctor </summary>
        /// <param name="errorQueueName">Error queue name</param>
        /// <param name="auditQueueName">Audit queue name</param>
        /// <param name="monitoringQueueName">Monitoring queue name</param>
        /// <param name="serviceControlQueueName">Service control queue</param>
        public QueueConventions(string errorQueueName,
                                string auditQueueName,
                                string monitoringQueueName,
                                string serviceControlQueueName)
        {
            ErrorQueueName = errorQueueName;
            AuditQueueName = auditQueueName;
            MonitoringQueueName = monitoringQueueName;
            ServiceControlQueueName = serviceControlQueueName;
        }

        /// <summary>
        /// Error queue name
        /// </summary>
        public string ErrorQueueName { get; }

        /// <summary>
        /// Audit queue name
        /// </summary>
        public string AuditQueueName { get; }

        /// <summary>
        /// Monitoring queue name
        /// Equals to Monitoring service instance name
        /// </summary>
        public string MonitoringQueueName { get; }

        /// <summary>
        /// Service control queue
        /// Equals to ServiceControl service instance name
        /// </summary>
        public string ServiceControlQueueName { get; }
    }
}