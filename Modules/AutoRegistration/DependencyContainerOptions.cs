namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
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
    }
}