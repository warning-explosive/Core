namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Abstractions;
    using Internals;

    /// <summary>
    /// DependencyContainer creation options
    /// </summary>
    public class DependencyContainerOptions
    {
        /// <summary>
        /// Excluded assemblies
        /// Assemblies excluded from type loading
        /// These assemblies and their types will be identified as third party and won't participate in the container registrations
        /// </summary>
        public IReadOnlyCollection<Assembly> ExcludedAssemblies { get; set; } = Array.Empty<Assembly>();

        /// <summary>
        /// Excluded namespaces
        /// Namespaces excluded from type loading
        /// These types will be identified as third party and won't participate in the container registrations
        /// </summary>
        public IReadOnlyCollection<string> ExcludedNamespaces { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Manual registrations
        /// </summary>
        public IReadOnlyCollection<IManualRegistration> ManualRegistrations { get; set; } = Array.Empty<IManualRegistration>();

        /// <summary>
        /// Generates IManualRegistration object with specified delegate
        /// </summary>
        /// <param name="registrationAction">Action with IRegistrationContainer instance</param>
        /// <returns>IManualRegistration instance</returns>
        public static IManualRegistration DelegateRegistration(Action<IManualRegistrationsContainer> registrationAction)
        {
            return new ManualDelegateRegistration(registrationAction);
        }
    }
}