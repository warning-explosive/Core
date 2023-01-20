namespace SpaceEngineers.Core.CrossCuttingConcerns.Logging
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Logging extensions
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Log critical information that leads to application failure
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="exception">Exception</param>
        public static void Critical(this ILogger logger, Exception exception)
        {
            logger.LogCritical(exception, null);
        }

        /// <summary>
        /// Log critical information that leads to application failure
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="exception">Exception</param>
        /// <param name="message">Message</param>
        /// <param name="args">Message args</param>
        public static void Critical(this ILogger logger, Exception exception, string message, params object[] args)
        {
            logger.LogCritical(exception, message, args);
        }

        /// <summary>
        /// Log caught error
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="exception">Exception</param>
        public static void Error(this ILogger logger, Exception exception)
        {
            logger.LogError(exception, null);
        }

        /// <summary>
        /// Log caught error
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="exception">Exception</param>
        /// <param name="message">Message</param>
        /// <param name="args">Message args</param>
        public static void Error(this ILogger logger, Exception exception, string message, params object[] args)
        {
            logger.LogError(exception, message, args);
        }

        /// <summary>
        /// Log warning
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="message">Message template</param>
        /// <param name="args">Message arguments</param>
        public static void Warning(this ILogger logger, string message, params object[] args)
        {
            logger.LogWarning(message, args);
        }

        /// <summary>
        /// Log information
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="message">Message template</param>
        /// <param name="args">Message arguments</param>
        public static void Information(this ILogger logger, string message, params object[] args)
        {
            logger.LogInformation(message, args);
        }

        /// <summary>
        /// Log debug information for developers
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="message">Message template</param>
        /// <param name="args">Message arguments</param>
        public static void Debug(this ILogger logger, string message, params object[] args)
        {
            logger.LogDebug(message, args);
        }

        /// <summary>
        /// Log the most detailed information
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="message">Message template</param>
        /// <param name="args">Message arguments</param>
        public static void Trace(this ILogger logger, string message, params object[] args)
        {
            logger.LogTrace(message, args);
        }
    }
}