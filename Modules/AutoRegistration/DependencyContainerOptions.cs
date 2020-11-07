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
        /// Applies DependencyContainer registration rules to external dependencies
        /// Calls before container registration phase.
        /// </summary>
        public event EventHandler<RegistrationEventArgs>? OnRegistration;

        /// <summary>
        /// Registers external dependencies without applying DependencyContainer registration rules.
        /// Calls before container sealing.
        /// </summary>
        public event EventHandler<RegistrationEventArgs>? OnVerify;

        /// <summary>
        /// Verify container or not
        /// </summary>
        /// <remarks>Default: true</remarks>
        public bool VerifyContainer { get; set; } = true;

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

        internal void NotifyOnRegistration(IRegistrationContainer registration)
        {
            OnRegistration?.Invoke(this, new RegistrationEventArgs(registration));
        }

        internal void NotifyOnVerify(IRegistrationContainer registration)
        {
            OnVerify?.Invoke(this, new RegistrationEventArgs(registration));
        }
    }
}