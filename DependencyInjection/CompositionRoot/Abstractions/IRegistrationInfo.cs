namespace SpaceEngineers.Core.CompositionRoot.Abstractions
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
        /// EnLifestyle
        /// </summary>
        EnLifestyle Lifestyle { get; }
    }
}