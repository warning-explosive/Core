namespace SpaceEngineers.Core.IntegrationTransport.InMemory
{
    using System.Reflection;
    using Api;
    using Basics;

    /// <summary>
    /// Identity
    /// </summary>
    public static class Identity
    {
        /// <summary>
        /// InMemory transport assembly
        /// </summary>
        public static Assembly Assembly { get; } = AssembliesExtensions.FindRequiredAssembly(
            AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(IntegrationTransport), nameof(InMemory)));

        /// <summary>
        /// InMemory transport identity
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>TransportIdentity</returns>
        public static TransportIdentity TransportIdentity(string name = "InMemoryIntegrationTransport") => new TransportIdentity(name, Assembly);
    }
}