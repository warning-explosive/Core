namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Abstractions;

    /// <summary>
    /// DependencyContainer creation options
    /// </summary>
    public class DependencyContainerOptions
    {
        /// <summary>
        /// Callback to register external dependencies. Called before container sealing.
        /// </summary>
        public Action<IRegistrationContainer>? RegistrationCallback { get; set; }

        /// <summary>
        /// SearchOption for assemblies in BaseDirectory
        /// Default: SearchOption.TopDirectoryOnly
        /// </summary>
        public SearchOption SearchOption { get; set; } = SearchOption.TopDirectoryOnly;

        /// <summary>
        /// Excluded namespaces
        /// Namespaces excluded from type loading
        /// These types will be identified as third party and won't participate in the container registrations
        /// </summary>
        // TODO: test different namespaces for service and impl
        public IReadOnlyCollection<string>? ExcludedNamespaces { get; set; }
    }
}