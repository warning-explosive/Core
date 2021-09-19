namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions.Internals
{
    using System.Collections.Generic;
    using Contract.Abstractions;

    /// <summary>
    /// ITestIntegrationContext
    /// </summary>
    public interface ITestIntegrationContext
    {
        /// <summary>
        /// Messages
        /// </summary>
        IReadOnlyCollection<IIntegrationMessage> Messages { get; }
    }
}