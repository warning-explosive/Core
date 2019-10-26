namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Diagnostics;
    using Enumerations;
    using SimpleInjector;

    /// <summary>
    /// ServiceRegistrationInfo
    /// </summary>
    [DebuggerDisplay("{ComponentType.FullName} - {ServiceType.FullName} - {EnLifestyle}")]
    internal class ServiceRegistrationInfo
    {
        /// <summary> .ctor </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="componentType">Component type</param>
        /// <param name="enLifestyle">EnLifestyle</param>
        internal ServiceRegistrationInfo(Type serviceType,
                                         Type componentType,
                                         EnLifestyle enLifestyle)
        {
            ServiceType = serviceType.IsGenericType
                              ? serviceType.GetGenericTypeDefinition()
                              : serviceType;
            ComponentType = componentType;
            Lifestyle = MapLifestyle(enLifestyle);
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
        /// Lifestyle
        /// </summary>
        internal Lifestyle Lifestyle { get; }
        
        private static Lifestyle MapLifestyle(EnLifestyle enLifestyle)
        {
            switch (enLifestyle)
            {
                case EnLifestyle.Transient: return Lifestyle.Transient;
                case EnLifestyle.Singleton: return Lifestyle.Singleton;
                case EnLifestyle.Scoped:    return Lifestyle.Scoped;
                default:                    return Lifestyle.Transient;
            }
        }
    }
}