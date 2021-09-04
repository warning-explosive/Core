namespace SpaceEngineers.Core.AutoRegistration.Api.Abstractions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Services provider
    /// </summary>
    public interface IAutoRegistrationServicesProvider : IResolvable
    {
        /// <summary>
        /// Gets resolvable services registered in container by AutoRegistration.Api include Unregistered and ManualRegistered types
        /// </summary>
        /// <returns>Resolvable types registered in container by AutoRegistration.Api</returns>
        IEnumerable<Type> Resolvable();

        /// <summary>
        /// Gets collection resolvable services registered in container by AutoRegistration.Api include Unregistered and ManualRegistered types
        /// </summary>
        /// <returns>Collection resolvable types registered in container by AutoRegistration.Api</returns>
        IEnumerable<Type> Collections();

        /// <summary>
        /// Gets external services registered in container by AutoRegistration.Api include Unregistered and ManualRegistered types
        /// </summary>
        /// <returns>External services registered in container by AutoRegistration.Api</returns>
        IEnumerable<Type> External();

        /// <summary>
        /// Gets decorators registered in container by AutoRegistration.Api include Unregistered and ManualRegistered types
        /// </summary>
        /// <returns>Decorators registered by AutoRegistration.Api</returns>
        IEnumerable<Type> Decorators();
    }
}