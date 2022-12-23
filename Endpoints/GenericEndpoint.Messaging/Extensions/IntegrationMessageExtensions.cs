namespace SpaceEngineers.Core.GenericEndpoint.Messaging.Extensions
{
    using Contract.Extensions;

    /// <summary>
    /// IntegrationMessage extensions
    /// </summary>
    public static class IntegrationMessageExtensions
    {
        /// <summary>
        /// Does IntegrationMessage represent an IIntegrationCommand
        /// </summary>
        /// <param name="message">IntegrationMessage</param>
        /// <returns>IntegrationMessage type is an IIntegrationCommand or not</returns>
        public static bool IsCommand(this IntegrationMessage message)
        {
            return message.ReflectedType.IsCommand();
        }

        /// <summary>
        /// Does IntegrationMessage represent an IIntegrationEvent
        /// </summary>
        /// <param name="message">IntegrationMessage</param>
        /// <returns>IntegrationMessage type is an IIntegrationEvent or not</returns>
        public static bool IsEvent(this IntegrationMessage message)
        {
            return message.ReflectedType.IsEvent();
        }

        /// <summary>
        /// Does IntegrationMessage represent an IIntegrationQuery
        /// </summary>
        /// <param name="message">IntegrationMessage</param>
        /// <returns>IntegrationMessage type is an IIntegrationQuery or not</returns>
        public static bool IsQuery(this IntegrationMessage message)
        {
            return message.ReflectedType.IsQuery();
        }

        /// <summary>
        /// Does IntegrationMessage represent an IIntegrationReply
        /// </summary>
        /// <param name="message">IntegrationMessage</param>
        /// <returns>IntegrationMessage type is an IIntegrationReply or not</returns>
        public static bool IsReply(this IntegrationMessage message)
        {
            return message.ReflectedType.IsReply();
        }

        /// <summary>
        /// Does IntegrationMessage represent an IIntegrationReply on IIntegrationQuery
        /// </summary>
        /// <param name="reply">Reply message</param>
        /// <param name="query">Query message</param>
        /// <returns>IntegrationMessage is an IIntegrationReply on IIntegrationQuery or not</returns>
        public static bool IsReplyOnQuery(this IntegrationMessage reply, IntegrationMessage query)
        {
            return reply.ReflectedType.IsReplyOnQuery(query.ReflectedType);
        }
    }
}