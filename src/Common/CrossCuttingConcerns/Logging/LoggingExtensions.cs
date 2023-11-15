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
            logger.Log(LogLevel.Critical, 0, exception, null, Array.Empty<object>());
        }

        /// <summary>
        /// Log critical information that leads to application failure
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="exception">Exception</param>
        /// <param name="message">Message</param>
        public static void Critical(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogLevel.Critical, 0, exception, message, Array.Empty<object>());
        }

        /// <summary>
        /// Log caught error
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="exception">Exception</param>
        public static void Error(this ILogger logger, Exception exception)
        {
            logger.Log(LogLevel.Error, 0, exception, null, Array.Empty<object>());
        }

        /// <summary>
        /// Log caught error
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="exception">Exception</param>
        /// <param name="message">Message</param>
        public static void Error(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogLevel.Error, 0, exception, message, Array.Empty<object>());
        }

        /// <summary>
        /// Log warning
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="message">Message template</param>
        public static void Warning(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Warning, 0, message, Array.Empty<object>());
        }

        /// <summary>
        /// Log information
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="message">Message template</param>
        public static void Information(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Information, 0, message, Array.Empty<object>());
        }

        /// <summary>
        /// Log debug information for developers
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="message">Message template</param>
        public static void Debug(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Debug, 0, message, Array.Empty<object>());
        }

        /// <summary>
        /// Log the most detailed information
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="message">Message template</param>
        public static void Trace(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Trace, 0, message, Array.Empty<object>());
        }
    }
}