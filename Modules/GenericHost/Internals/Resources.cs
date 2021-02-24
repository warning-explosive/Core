namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    internal static class Resources
    {
        internal const string StartedSuccessfully = "{0} started successfully";
        internal const string WaitingForIncomingMessages = "Waiting for incoming messages...";

        private static readonly IReadOnlyDictionary<string, int> Map
            = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                [StartedSuccessfully] = 0,
                [WaitingForIncomingMessages] = 1
            };

        // TODO: ILogger extensions
        internal static void Information(
            this ILogger logger,
            string message,
            params object[] args)
        {
            logger.Log(LogLevel.Information, Map[message], message, args);
        }

        internal static void Error(
            this ILogger logger,
            Exception exception,
            string message,
            params object[] args)
        {
            logger.Log(LogLevel.Error, exception, message, args);
        }

        internal static void Error(
            this ILogger logger,
            Exception exception)
        {
            logger.Log(LogLevel.Error, exception, string.Empty);
        }
    }
}