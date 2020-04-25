namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections.Generic;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Dependency container abstraction
    /// </summary>
    public interface IDependencyContainer
    {
        /// <summary>
        /// Resolve service implementation
        /// </summary>
        /// <typeparam name="T">IResolvable</typeparam>
        /// <returns>Service implementation</returns>
        T Resolve<T>()
            where T : class, IResolvable;

        /// <summary>
        /// Resolve untyped service implementation
        /// </summary>
        /// <param name="serviceType">IResolvable</param>
        /// <returns>Untyped service implementation</returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// Resolve concrete implementation
        /// </summary>
        /// <typeparam name="T">IResolvable</typeparam>
        /// <returns>Service implementation</returns>
        T ResolveImplementation<T>()
            where T : class, IResolvableImplementation;

        /// <summary>
        /// Resolve untyped concrete implementation
        /// </summary>
        /// <param name="concreteImplementationType">IResolvableImplementation</param>
        /// <returns>Untyped concrete implementation</returns>
        object ResolveImplementation(Type concreteImplementationType);

        /// <summary>
        /// Resolve service implementations collection
        /// </summary>
        /// <typeparam name="T">IResolvable</typeparam>
        /// <returns>Service implementation</returns>
        IEnumerable<T> ResolveCollection<T>()
            where T : class, ICollectionResolvable;

        /// <summary>
        /// Resolve untyped service implementations collection
        /// </summary>
        /// <param name="serviceType">IResolvable</param>
        /// <returns>Untyped service implementation</returns>
        IEnumerable<object> ResolveCollection(Type serviceType);

        /// <summary>
        /// Resolve external service implementation
        /// </summary>
        /// <typeparam name="TExternalService">External service abstraction</typeparam>
        /// <returns>Our implementation of external service</returns>
        TExternalService ResolveExternal<TExternalService>()
            where TExternalService : class;

        /// <summary>
        /// Resolve external service implementation
        /// </summary>
        /// <param name="externalServiceType">External service abstraction</param>
        /// <returns>Our implementation of external service</returns>
        object ResolveExternal(Type externalServiceType);
    }
}