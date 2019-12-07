namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Diagnostics;
    using Attributes;
    using Basics.Exceptions;
    using Enumerations;
    using SimpleInjector;

    /// <summary>
    /// ServiceRegistrationInfo
    /// </summary>
    [DebuggerDisplay("{ComponentType.FullName} - {ServiceType.FullName} - {Lifestyle} - {Attribute}")]
    internal class ServiceRegistrationInfo
    {
        /// <summary> .ctor </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="componentType">Component type</param>
        /// <param name="lifestyle">EnLifestyle</param>
        internal ServiceRegistrationInfo(Type serviceType,
                                         Type componentType,
                                         EnLifestyle? lifestyle)
        {
            ServiceType = serviceType.IsGenericType
                              ? serviceType.GetGenericTypeDefinition()
                              : serviceType;
            ComponentType = componentType;

            if (lifestyle == null)
            {
                throw new AttributeRequiredException(typeof(LifestyleAttribute), componentType);
            }

            Lifestyle = LifeStyleMapper.MapLifestyle(lifestyle.Value);
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

        /// <summary>
        /// Conditional attribute
        /// </summary>
        internal Type? Attribute { get; set; }
    }
}