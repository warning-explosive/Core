namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
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
    }
}