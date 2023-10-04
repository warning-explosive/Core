namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ
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
        /// RabbitMq transport assembly
        /// </summary>
        public static Assembly Assembly { get; } = AssembliesExtensions.FindRequiredAssembly(
            AssembliesExtensions.BuildName(
                nameof(SpaceEngineers),
                nameof(Core),
                nameof(IntegrationTransport),
                nameof(RabbitMQ)));

        /// <summary>
        /// RabbitMq transport identity
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>TransportIdentity</returns>
        public static TransportIdentity TransportIdentity(string name = "RabbitMqIntegrationTransport") => new TransportIdentity(name, Assembly);
    }
}