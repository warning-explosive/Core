namespace SpaceEngineers.Core.AutoRegistration.Abstractions
{
    using System;

    /// <summary>
    /// Abstraction for dependency container that supports service versions
    /// </summary>
    public interface IVersionedContainer
    {
        /// <summary>
        /// Opens scope with specified service version
        /// </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TImplementation">TImplementation type-argument</typeparam>
        /// <returns>Scope cleanup</returns>
        IDisposable UseVersion<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        /// <summary>
        /// Pull actual applied version for service
        /// </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>Applied version for service or null if default version used</returns>
        Type? AppliedVersion<TService>()
            where TService : class;
    }
}