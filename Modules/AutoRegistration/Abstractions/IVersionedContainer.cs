namespace SpaceEngineers.Core.AutoRegistration.Abstractions
{
    using System;
    using AutoWiringApi.Abstractions;
    using Internals;

    /// <summary>
    /// Abstraction for dependency container that supports service versions
    /// </summary>
    public interface IVersionedContainer
    {
        /// <summary>
        /// Opens scope with specified service version
        /// </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TVersion">TVersion type-argument</typeparam>
        /// <returns>Scope cleanup</returns>
        IDisposable UseVersion<TService, TVersion>()
            where TService : class
            where TVersion : class, TService, IVersionFor<TService>;

        /// <summary>
        /// Opens scope with specified service version
        /// </summary>
        /// <param name="versionFactory">Version instance factory</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>Scope cleanup</returns>
        IDisposable UseVersion<TService>(Func<IVersionFor<TService>> versionFactory)
            where TService : class;

        /// <summary>
        /// Pull actual applied version for service
        /// </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>Applied version for service or null if default version used</returns>
        VersionInfo? AppliedVersion<TService>()
            where TService : class;
    }
}