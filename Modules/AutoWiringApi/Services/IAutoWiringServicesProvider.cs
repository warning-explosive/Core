namespace SpaceEngineers.Core.AutoWiringApi.Services
{
    using System;
    using System.Collections.Generic;
    using Abstractions;

    /// <summary>
    /// Services provider
    /// </summary>
    public interface IAutoWiringServicesProvider : IResolvable
    {
        /// <summary>
        /// Gets resolvable services registered in container by AutoWiring.API include Unregistered and ManualRegistered types
        /// </summary>
        /// <returns>Resolvable types registered in container by AutoWiring.API</returns>
        IEnumerable<Type> Resolvable();

        /// <summary>
        /// Gets collection resolvable services registered in container by AutoWiring.API include Unregistered and ManualRegistered types
        /// </summary>
        /// <returns>Collection resolvable types registered in container by AutoWiring.API</returns>
        IEnumerable<Type> Collections();

        /// <summary>
        /// Gets external services registered in container by AutoWiring.API include Unregistered and ManualRegistered types
        /// </summary>
        /// <returns>External services registered in container by AutoWiring.API</returns>
        IEnumerable<Type> External();

        /// <summary>
        /// Gets services which has explicitly defined versions and registered in container by AutoWiring.API include Unregistered and ManualRegistered types
        /// </summary>
        /// <returns>External services registered in container by AutoWiring.API</returns>
        IEnumerable<Type> Versions();

        /// <summary>
        /// Gets decorators registered in container by AutoWiring.API include conditional decorators, Unregistered and ManualRegistered types
        /// </summary>
        /// <returns>Decorators registered by AutoWiring.API include conditional decorators</returns>
        IEnumerable<Type> Decorators();

        /// <summary>
        /// Gets decorators for collections registered in container by AutoWiring.API include conditional decorators, Unregistered and ManualRegistered types
        /// </summary>
        /// <returns>Decorators for collections registered by AutoWiring.API include conditional decorators</returns>
        IEnumerable<Type> CollectionDecorators();
    }
}