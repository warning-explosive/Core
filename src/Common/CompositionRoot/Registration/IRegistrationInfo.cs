namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using AutoRegistration.Api.Enumerations;

    /// <summary>
    /// IRegistrationInfo
    /// </summary>
    public interface IRegistrationInfo
    {
        /// <summary>
        /// Service
        /// </summary>
        Type Service { get; }

        /// <summary>
        /// Lifestyle
        /// </summary>
        EnLifestyle Lifestyle { get; }
    }
}