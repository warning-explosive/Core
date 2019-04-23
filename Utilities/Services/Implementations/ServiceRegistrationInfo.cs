namespace SpaceEngineers.Core.Utilities.Services.Implementations
{
    using System;
    using Enumerations;

    /// <summary>
    /// ServiceRegistrationInfo
    /// </summary>
    internal class ServiceRegistrationInfo
    {
        /// <summary> .ctor </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="componentType">Component type</param>
        /// <param name="enLifestyle">EnLifestyle</param>
        public ServiceRegistrationInfo(Type serviceType,
                                       Type componentType,
                                       EnLifestyle enLifestyle)
        {
            ServiceType = serviceType;
            ComponentType = componentType;
            EnLifestyle = enLifestyle;
        }

        /// <summary>
        /// Component type
        /// </summary>
        internal Type ComponentType { get; }
        
        /// <summary>
        /// Service type
        /// </summary>
        internal Type ServiceType { get; }

        /// <summary>
        /// EnLifestyle
        /// </summary>
        internal EnLifestyle EnLifestyle { get; }
    }
}