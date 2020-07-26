namespace SpaceEngineers.Core.AutoWiringApi.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents abstraction for services that support versions
    /// </summary>
    /// <typeparam name="TService">TService type-argument</typeparam>
    public interface IVersionedService<TService>
        where TService : class
    {
        /// <summary>
        /// Current version of service selected by settings
        /// </summary>
        TService Current { get; }

        /// <summary>
        /// Original version of service defined in code
        /// </summary>
        TService Original { get; }

        /// <summary>
        /// All versions of service registered in the dependency container
        /// </summary>
        ICollection<TService> Versions { get; }
    }
}