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
        /// Reads optional header
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="header">Header name</param>
        /// <typeparam name="THeader">THeader type-argument</typeparam>
        /// <returns>Optional header</returns>
        public static THeader? ReadHeader<THeader>(this IntegrationMessage message, string header)
        {
            return message.Headers.TryGetValue(header, out var headerValue)
                   && headerValue is THeader typedHeaderValue
                ? typedHeaderValue
                : default;
        }

        /// <summary>
        /// Reads required header
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="header">Header name</param>
        /// <typeparam name="THeader">THeader type-argument</typeparam>
        /// <returns>Required header</returns>
        public static THeader ReadRequiredHeader<THeader>(this IntegrationMessage message, string header)
        {
            return message.Headers.TryGetValue(header, out var headerValue)
                   && headerValue is THeader typedHeaderValue
                ? typedHeaderValue
                : throw new InvalidOperationException($"Message should have {header} message header");
        }

        /// <summary>
        /// Sets actual delivery date to the input queue
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="actualDeliveryDate">Actual delivery date</param>
        public static void SetActualDeliveryDate(this IntegrationMessage message, DateTime actualDeliveryDate)
        {
            message.Headers[IntegratedMessageHeader.ActualDeliveryDate] = actualDeliveryDate;
        }

        /// <summary>
        /// Sets a date as a header value that defers delivery of a message until it arrives
        /// The message will be delivered no earlier than this date and not exactly in that time due to asynchronous nature of the messaging
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="dueTime">Time that transport waits before deliver message again</param>
        public static void DeferDelivery(this IntegrationMessage message, TimeSpan dueTime)
        {
            message.Headers[IntegratedMessageHeader.DeferredUntil] = DateTime.Now + dueTime;
        }

        /// <summary>
        /// Returns attribute that message is deferred or not
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>Message is deferred or not</returns>
        public static bool IsDeferred(this IntegrationMessage message)
        {
            return message.Headers.ContainsKey(IntegratedMessageHeader.DeferredUntil);
        }

        /// <summary>
        /// Increment retry counter
        /// </summary>
        /// <param name="message">Integration message</param>
        public static void IncrementRetryCounter(this IntegrationMessage message)
        {
            var actualCounter = message.ReadHeader<int>(IntegratedMessageHeader.RetryCounter);

            message.Headers[IntegratedMessageHeader.RetryCounter] = actualCounter + 1;
        }

        /// <summary>
        /// Mark message as replied to the query
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <exception cref="InvalidOperationException">Replied header already set</exception>
        public static void MarkAsReplied(this IntegrationMessage message)
        {
            if (!message.IsQuery())
            {
                throw new InvalidOperationException($"{message.ReflectedType} is not a query. You can reply only on queries.");
            }

            if (message.DidHandlerReply())
            {
                throw new InvalidOperationException("Message handler already replied to integration query");
            }

            message.Headers[IntegratedMessageHeader.DidHandlerReplyToTheQuery] = true;
        }

        /// <summary>
        /// Did the handler reply to the query
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>Handler replied or not</returns>
        public static bool DidHandlerReply(this IntegrationMessage message)
        {
            return message.Headers.TryGetValue(IntegratedMessageHeader.DidHandlerReplyToTheQuery, out var value)
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

        internal static bool IsEvent(this IntegrationMessage message)
        {
            return typeof(IIntegrationEvent).IsAssignableFrom(message.ReflectedType);
        }
    }
}