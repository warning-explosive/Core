namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    internal static class Resources
    {
        internal const string StartedSuccessfully = "{0} started successfully";
        internal const string WaitingForIncomingMessages = "Waiting for incoming messages...";
        internal const string DispatchMessage = "Dispatch '{0}' message on thread {1}";

        private static readonly IReadOnlyDictionary<string, int> Map
            = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                [StartedSuccessfully] = 0,
                [WaitingForIncomingMessages] = 1,
                [DispatchMessage] = 2,
            };

        internal static void Information(
            this ILogger logger,
            string message,
            params object[] args)
        {
            logger.Log(LogLevel.Information, Map[message], message, args);
        }
    }
}