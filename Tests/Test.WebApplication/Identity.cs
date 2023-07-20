namespace SpaceEngineers.Core.Test.WebApplication
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using GenericEndpoint.Contract;

    /// <summary>
    /// Identity
    /// </summary>
    [SuppressMessage("Analysis", "CA1724", Justification = "desired name")]
    public static class Identity
    {
        /// <summary>
        /// WebGateway logical name
        /// </summary>
        public const string LogicalName = "WebGateway";

        /// <summary>
        /// WebGateway assembly
        /// </summary>
        public static Assembly Assembly { get; } = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Unable to get entry assembly");

        /// <summary>
        /// WebGateway identity
        /// </summary>
        public static EndpointIdentity EndpointIdentity { get; } = new EndpointIdentity(LogicalName, Assembly);
    }
}