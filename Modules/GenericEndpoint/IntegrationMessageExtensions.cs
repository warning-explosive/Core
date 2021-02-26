namespace SpaceEngineers.Core.GenericEndpoint
{
    using System;
    using Basics;
    using Contract.Abstractions;

    /// <summary>
    /// IntegrationMessage extensions
    /// </summary>
    public static class IntegrationMessageExtensions
    {
        /// <summary>
        /// Increment retry counter
        /// </summary>
        /// <param name="message">Integration message</param>
        public static void IncrementRetryCounter(this IntegrationMessage message)
        {
            var actualCounter = message.ReadRetryCounter();

            message.Headers[IntegratedMessageHeader.MessageRetryCounter] = actualCounter + 1;
        }

        /// <summary>
        /// Read actual retry counter
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>Actual retry counter</returns>
        public static int ReadRetryCounter(this IntegrationMessage message)
        {
            return message.Headers.TryGetValue(IntegratedMessageHeader.MessageRetryCounter, out var value)
                   && value is int retryCounter
                ? retryCounter
                : 0;
        }

        /// <summary>
        /// Set replied header
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <exception cref="InvalidOperationException">Replied header already set</exception>
        public static void SetReplied(this IntegrationMessage message)
        {
            if (!message.IsQuery())
            {
                throw new InvalidOperationException($"{message.ReflectedType} is not a query. You can reply only on queries.");
            }

            if (message.HandlerReplied())
            {
                throw new InvalidOperationException("Message handler already replied to integration query");
            }

            message.Headers[IntegratedMessageHeader.HandlerRepliedToTheQuery] = true;
        }

        /// <summary>
        /// Is handler replied
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>Handler replied header value</returns>
        public static bool HandlerReplied(this IntegrationMessage message)
        {
            return message.Headers.TryGetValue(IntegratedMessageHeader.HandlerRepliedToTheQuery, out var value)
                   && value is bool handlerReplied
                   && handlerReplied;
        }

        internal static bool IsCommand(this IntegrationMessage message)
        {
            return typeof(IIntegrationCommand).IsAssignableFrom(message.ReflectedType);
        }

        internal static bool IsQuery(this IntegrationMessage message)
        {
            return message.ReflectedType.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>));
        }
    }
}