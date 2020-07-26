namespace SpaceEngineers.Core.AutoRegistration.Abstractions
{
    using System;
    using System.Collections.Generic;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Dependency container abstraction
    /// </summary>
    public interface IDependencyContainer : IScopedContainer,
                                            IVersionedContainer,
                                            IResolvable
    {
        /// <summary>
        /// Resolve service implementation
        /// </summary>
        /// <typeparam name="T">IResolvable</typeparam>
        /// <returns>Service implementation</returns>
        T Resolve<T>()
            where T : class;

        /// <summary>
        /// Resolve untyped service implementation
        /// </summary>
        /// <param name="serviceType">IResolvable</param>
        /// <returns>Untyped service implementation</returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// Resolve service implementations collection
        /// </summary>
        /// <typeparam name="T">IResolvable</typeparam>
        /// <returns>Service implementation</returns>
        IEnumerable<T> ResolveCollection<T>()
            where T : class;

        /// <summary>
        /// Resolve untyped service implementations collection
        /// </summary>
        /// <param name="serviceType">IResolvable</param>
        /// <returns>Untyped service implementation</returns>
        IEnumerable<object> ResolveCollection(Type serviceType);
    }
}