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
        /// <returns>Integration message with incremented retry counter</returns>
        public static IntegrationMessage IncrementRetryCounter(this IntegrationMessage message)
        {
            var actualCounter = message.ReadRetryCounter();

            message.Headers[IntegratedMessageHeader.MessageRetryCounter] = actualCounter + 1;

            return message;
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
        /// <returns>Set integration message</returns>
        /// <exception cref="InvalidOperationException">Replied header already set</exception>
        public static IntegrationMessage SetReplied(this IntegrationMessage message)
        {
            if (message.HandlerReplied())
            {
                throw new InvalidOperationException("Message handler already replied to integration query");
            }

            message.Headers[IntegratedMessageHeader.HandlerRepliedToTheQuery] = true;

            return message;
        }

        internal static bool HandlerReplied(this IntegrationMessage message)
        {
            return message.Headers.TryGetValue(IntegratedMessageHeader.HandlerRepliedToTheQuery, out var value)
                   && value is bool handlerReplied
                   && handlerReplied;
        }

        internal static bool IsQuery(this IntegrationMessage message)
        {
            return message.ReflectedType.IsSubclassOfOpenGeneric(typeof(IIntegrationQuery<>));
        }
    }
}