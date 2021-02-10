namespace SpaceEngineers.Core.GenericHost
{
    using System.Reflection;

    /// <summary>
    /// Endpoint initialization options
    /// </summary>
    public class EndpointOptions
    {
        /// <summary> .cctor </summary>
        /// <param name="identity">Endpoint identity</param>
        /// <param name="assembly">Endpoint assembly</param>
        public EndpointOptions(EndpointIdentity identity, Assembly assembly)
        {
            Identity = identity;
            Assembly = assembly;
        }

        /// <summary>
        /// Endpoint identity
        /// </summary>
        public EndpointIdentity Identity { get; }

        /// <summary>
        /// Endpoint assembly that limits assembly loading for endpoint's dependency container
        /// </summary>
        public Assembly Assembly { get; }
    }
}