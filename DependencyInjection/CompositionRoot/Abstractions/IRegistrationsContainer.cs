namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using Registration;

    /// <summary>
    /// IRegistrationsContainer
    /// </summary>
    public interface IRegistrationsContainer : IResolvable
    {
        /// <summary> Singletons </summary>
        /// <returns> Singleton objects </returns>
        IEnumerable<(Type Type, object Instance)> Singletons();

        /// <summary> Resolvable </summary>
        /// <returns> ServiceRegistrationInfos </returns>
        IEnumerable<ServiceRegistrationInfo> Resolvable();

        /// <summary> Delegates </summary>
        /// <returns> DelegateRegistrationInfos </returns>
        IEnumerable<DelegateRegistrationInfo> Delegates();

        /// <summary> Collections </summary>
        /// <returns>ServiceRegistrationInfos</returns>
        IEnumerable<ServiceRegistrationInfo> Collections();

        /// <summary> Decorators </summary>
        /// <returns>DecoratorRegistrationInfos</returns>
        IEnumerable<DecoratorRegistrationInfo> Decorators();
    }
}