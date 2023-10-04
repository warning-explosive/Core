namespace SpaceEngineers.Core.Test.WebApplication
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Basics;
    using GenericEndpoint.Contract;

    /// <summary>
    /// Identity
    /// </summary>
    [SuppressMessage("Analysis", "CA1724", Justification = "desired name")]
    public static class Identity
    {
        /// <summary>
        /// TestEndpoint logical name
        /// </summary>
        public const string LogicalName = "TestEndpoint";

        /// <summary>
        /// TestEndpoint assembly
        /// </summary>
        public static Assembly Assembly { get; } = AssembliesExtensions.FindRequiredAssembly(
            AssembliesExtensions.BuildName(
                nameof(SpaceEngineers),
                nameof(Core),
                nameof(Test),
                nameof(WebApplication)));

        /// <summary>
        /// TestEndpoint identity
        /// </summary>
        public static EndpointIdentity EndpointIdentity { get; } = new EndpointIdentity(LogicalName, Assembly);
    }
}