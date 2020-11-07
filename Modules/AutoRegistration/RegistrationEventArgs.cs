namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using Abstractions;

    /// <summary>
    /// RegistrationEventArgs
    /// </summary>
    public class RegistrationEventArgs : EventArgs
    {
        /// <summary> .cctor </summary>
        /// <param name="registration">IRegistrationContainer</param>
        public RegistrationEventArgs(IRegistrationContainer registration)
        {
            Registration = registration;
        }

        /// <summary>
        /// IRegistrationContainer
        /// </summary>
        public IRegistrationContainer Registration { get; }
    }
}