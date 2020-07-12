namespace SpaceEngineers.Core.AutoWiringApi.Abstractions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Services provider
    /// </summary>
    public interface IAutoWiringServicesProvider : IResolvable
    {
        /// <summary>
        /// Get resolvable types registered in container by AutoWiring.API include Unregistered types
        /// </summary>
        /// <returns>Resolvable types registered in container by AutoWiring.API</returns>
        IEnumerable<Type> Resolvable();

        /// <summary>
        /// Get collection resolvable types registered in container by AutoWiring.API include Unregistered types
        /// </summary>
        /// <returns>Collection resolvable types registered in container by AutoWiring.API</returns>
        IEnumerable<Type> Collections();

        /// <summary>
        /// Get exact types registered in container by AutoWiring.API include Unregistered types
        /// </summary>
        /// <returns>Exact types registered in container by AutoWiring.API</returns>
        IEnumerable<Type> Implementations();

        /// <summary>
        /// Get external services registered in container by AutoWiring.API include Unregistered types
        /// </summary>
        /// <returns>External services registered in container by AutoWiring.API</returns>
        IEnumerable<Type> External();
    }
}