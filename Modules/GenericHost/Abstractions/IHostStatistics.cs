namespace SpaceEngineers.Core.GenericHost.Abstractions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ITransportStatistics abstraction
    /// </summary>
    public interface IHostStatistics
    {
        /// <summary>
        /// Dispatching errors
        /// </summary>
        IReadOnlyCollection<Exception> DispatchingErrors { get; }

        /// <summary>
        /// Registers dispatching exception
        /// </summary>
        /// <param name="exception">Dispatching exception</param>
        /// <returns>IHostStatistics</returns>
        public IHostStatistics Register(Exception exception);
    }
}