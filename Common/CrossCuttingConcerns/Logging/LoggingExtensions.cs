namespace SpaceEngineers.Core.CrossCuttingConcerns.Logging
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Logging extensions
    /// </summary>
    public static class LoggingExtensions
    {
        private static readonly Func<string?, Exception?, string> _messageFormatter = MessageFormatter;

        /// <summary>
        /// Log critical information that leads to application failure
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="exception">Exception</param>
        public static void Critical(this ILogger logger, Exception exception)
        {
            logger.Log(LogLevel.Critical, 0, null, exception, _messageFormatter);
        }

        /// <summary>
        /// Log critical information that leads to application failure
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="exception">Exception</param>
        /// <param name="message">Message</param>
        public static void Critical(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogLevel.Critical, 0, message, exception, _messageFormatter);
        }

        /// <summary>
        /// Log caught error
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="exception">Exception</param>
        public static void Error(this ILogger logger, Exception exception)
        {
            logger.Log(LogLevel.Error, 0, null, exception, _messageFormatter);
        }

        /// <summary>
        /// Log caught error
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="exception">Exception</param>
        /// <param name="message">Message</param>
        public static void Error(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogLevel.Error, 0, message, exception, _messageFormatter);
        }

        /// <summary>
        /// Log warning
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="message">Message template</param>
        public static void Warning(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Warning, 0, message, null, _messageFormatter);
        }

        /// <summary>
        /// Log information
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="message">Message template</param>
        public static void Information(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Information, 0, message, null, _messageFormatter);
        }

        /// <summary>
        /// Log debug information for developers
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="message">Message template</param>
        public static void Debug(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Debug, 0, message, null, _messageFormatter);
        }

        /// <summary>
        /// Log the most detailed information
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="message">Message template</param>
        public static void Trace(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Trace, 0, message, null, _messageFormatter);
        }

        private static string MessageFormatter(string? message, Exception? exception)
        {
            if (message != null)
            {
                return exception != null
                    ? $"{message}{Environment.NewLine}{exception}"
                    : message;
            }

            if (exception != null)
            {
                return exception.ToString();
            }

            return string.Empty;
        }
    }
}