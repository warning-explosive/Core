namespace SpaceEngineers.Core.GenericEndpoint.Host.Logging
{
    using System;
    using Contract;
    using Microsoft.Extensions.Logging;

    internal class LoggerDecorator : ILogger
    {
        private readonly ILogger _logger;
        private readonly EndpointIdentity _endpointIdentity;

        public LoggerDecorator(ILogger logger, EndpointIdentity endpointIdentity)
        {
            _logger = logger;
            _endpointIdentity = endpointIdentity;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, Formatter<TState>(_endpointIdentity));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return _logger.BeginScope(state);
        }

        private static Func<TState, Exception?, string> Formatter<TState>(
            EndpointIdentity endpointIdentity)
        {
            return (state, exception) =>
            {
                var message = exception == null
                    ? state.ToString()
                    : $"{state} -> {exception}";

                return $"{endpointIdentity} -> {message}";
            };
        }
    }
}
