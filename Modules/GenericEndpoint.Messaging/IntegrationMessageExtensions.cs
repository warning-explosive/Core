namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using Basics;
    using Contract.Abstractions;

    /// <summary>
    /// IntegrationMessage extensions
    /// </summary>
    public static class IntegrationMessageExtensions
    {
        /// <summary>
        /// Is the IntegrationMessage a command
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>IntegrationMessage is a command or not</returns>
        public static bool IsCommand(this IntegrationMessage message)
        {
            return typeof(IIntegrationCommand).IsAssignableFrom(message.ReflectedType);
        }

        /// <summary>
        /// Is the IntegrationMessage an event
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>IntegrationMessage is an event or not</returns>
        public static bool IsEvent(this IntegrationMessage message)
        {
            return typeof(IIntegrationEvent).IsAssignableFrom(message.ReflectedType);
        }

        /// <summary>
        /// Is the IntegrationMessage a reply
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>IntegrationMessage is a reply or not</returns>
        public static bool IsReply(this IntegrationMessage message)
        {
            return typeof(IIntegrationReply).IsAssignableFrom(message.ReflectedType);
        }

        /// <summary>
        /// Is the IntegrationMessage a reply
        /// </summary>
        /// <param name="reply">Integration reply</param>
        /// <param name="query">Integration query</param>
        /// <returns>IntegrationMessage is a reply or not</returns>
        public static bool IsReplyOnQuery(this IntegrationMessage reply, IntegrationMessage query)
        {
            return typeof(IIntegrationReply).IsAssignableFrom(reply.ReflectedType)
                && typeof(IIntegrationQuery<>).MakeGenericType(reply.ReflectedType).IsAssignableFrom(query.ReflectedType);
        }

        /// <summary>
        /// Is the IntegrationMessage a query
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>IntegrationMessage is a query or not</returns>
        public static bool IsQuery(this IntegrationMessage message)
        {
            return message.ReflectedType.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>));
        }
    }
}