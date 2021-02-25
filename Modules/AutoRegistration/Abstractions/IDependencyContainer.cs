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
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>Service implementation</returns>
        TService Resolve<TService>()
            where TService : class;

        /// <summary>
        /// Resolve service implementation and initialize it with runtime information
        /// </summary>
        /// <param name="runtimeInfo">Runtime information</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TRuntimeInfo">TRuntimeInfo type-argument</typeparam>
        /// <returns>Service implementation</returns>
        TService Resolve<TService, TRuntimeInfo>(TRuntimeInfo runtimeInfo)
            where TService : class, IInitializable<TRuntimeInfo>;

        /// <summary>
        /// Resolve untyped service implementation
        /// </summary>
        /// <param name="serviceType">Service type</param>
        /// <returns>Untyped service implementation</returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// Resolve service implementations collection
        /// </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>Service implementation</returns>
        IEnumerable<TService> ResolveCollection<TService>()
            where TService : class;

        /// <summary>
        /// Resolve untyped service implementations collection
        /// </summary>
        /// <param name="serviceType">Service type</param>
        /// <returns>Untyped service implementation</returns>
        IEnumerable<object> ResolveCollection(Type serviceType);
    }
}