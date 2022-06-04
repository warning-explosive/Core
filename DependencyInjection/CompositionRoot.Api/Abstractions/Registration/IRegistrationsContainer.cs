namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration
{
    using System.Collections.Generic;

    /// <summary>
    /// IRegistrationsContainer
    /// </summary>
    public interface IRegistrationsContainer
    {
        /// <summary> Instances </summary>
        /// <returns> Instance components </returns>
        IReadOnlyCollection<InstanceRegistrationInfo> Instances();

        /// <summary> Resolvable </summary>
        /// <returns> ServiceRegistrationInfos </returns>
        IReadOnlyCollection<ServiceRegistrationInfo> Resolvable();

        /// <summary> Delegates </summary>
        /// <returns> DelegateRegistrationInfos </returns>
        IReadOnlyCollection<DelegateRegistrationInfo> Delegates();

        /// <summary> Collections </summary>
        /// <returns>ServiceRegistrationInfos</returns>
        IReadOnlyCollection<IRegistrationInfo> Collections();

        /// <summary> Decorators </summary>
        /// <returns>DecoratorRegistrationInfos</returns>
        IReadOnlyCollection<DecoratorRegistrationInfo> Decorators();
    }
}