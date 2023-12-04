namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using Messaging;

    /// <summary>
    /// IMessagesCollector
    /// </summary>
    public interface IMessagesCollector
    {
        /// <summary>
        /// IMessagesCollector
        /// </summary>
        /// <param name="message">IntegrationMessage</param>
        void Collect(IntegrationMessage message);
    }
}