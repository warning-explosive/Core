namespace SpaceEngineers.Core.GenericHost.Api.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// IFrameworkDependenciesProvider
    /// </summary>
    public interface IFrameworkDependenciesProvider
    {
        /// <summary>
        /// Gets optional service by key
        /// </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>Optional service resolved by key</returns>
        TService? GetService<TService>();

        /// <summary>
        /// Gets required service by key
        /// </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>Required service resolved by key</returns>
        TService GetRequiredService<TService>()
            where TService : notnull;

        /// <summary>
        /// Gets service collection by key
        /// </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>Service collection resolved by key</returns>
        IEnumerable<TService> GetServices<TService>()
            where TService : notnull;
    }
}