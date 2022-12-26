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
        /// Does IntegrationMessage represent an IIntegrationRequest
        /// </summary>
        /// <param name="message">IntegrationMessage</param>
        /// <returns>IntegrationMessage type is an IIntegrationRequest or not</returns>
        public static bool IsRequest(this IntegrationMessage message)
        {
            return message.ReflectedType.IsRequest();
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
        /// Does IntegrationMessage represent an IIntegrationReply on IIntegrationRequest
        /// </summary>
        /// <param name="reply">Reply message</param>
        /// <param name="request">Request message</param>
        /// <returns>IntegrationMessage is an IIntegrationReply on IIntegrationRequest or not</returns>
        public static bool IsReplyOnRequest(this IntegrationMessage reply, IntegrationMessage request)
        {
            return reply.ReflectedType.IsReplyOnRequest(request.ReflectedType);
        }
    }
}