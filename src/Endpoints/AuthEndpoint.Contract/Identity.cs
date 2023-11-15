namespace SpaceEngineers.Core.AuthEndpoint.Contract
{
    using System.Reflection;
    using Basics;
    using GenericEndpoint.Contract;

    /// <summary>
    /// Identity
    /// </summary>
    public static class Identity
    {
        /// <summary>
        /// AuthEndpoint logical name
        /// </summary>
        public const string LogicalName = nameof(AuthEndpoint);

        /// <summary>
        /// AuthEndpoint assembly
        /// </summary>
        public static Assembly Assembly { get; } = AssembliesExtensions.FindRequiredAssembly(
            AssembliesExtensions.BuildName(
                nameof(SpaceEngineers),
                nameof(Core),
                nameof(AuthEndpoint)));

        /// <summary>
        /// AuthEndpoint identity
        /// </summary>
        public static EndpointIdentity EndpointIdentity { get; } = new EndpointIdentity(LogicalName, Assembly);
    }
}