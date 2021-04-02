namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Abstractions;

    internal class HostStatistics : IHostStatistics
    {
        private readonly ConcurrentBag<Exception> _dispatchingErrors;

        public HostStatistics()
        {
            _dispatchingErrors = new ConcurrentBag<Exception>();
        }

        public IReadOnlyCollection<Exception> DispatchingErrors => _dispatchingErrors;

        public IHostStatistics Register(Exception exception)
        {
            _dispatchingErrors.Add(exception);
            return this;
        }
    }
}