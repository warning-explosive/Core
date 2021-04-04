namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Abstractions;
    using GenericEndpoint;

    internal class HostStatistics : IHostStatistics
    {
        private readonly ConcurrentBag<FailedMessage> _failures;

        public HostStatistics()
        {
            _failures = new ConcurrentBag<FailedMessage>();
        }

        public IReadOnlyCollection<FailedMessage> FailedMessages => _failures;

        public IHostStatistics RegisterFailure(IntegrationMessage message, Exception exception)
        {
            _failures.Add(new FailedMessage(message, exception));
            return this;
        }
    }
}