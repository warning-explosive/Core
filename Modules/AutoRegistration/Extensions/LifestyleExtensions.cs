namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Basics.Exceptions;
    using SimpleInjector;

    /// <summary>
    /// Lifestyle extensions
    /// </summary>
    public static class LifestyleExtensions
    {
        /// <summary>
        /// Extracts lifestyle of the component
        /// </summary>
        /// <param name="type">Component type</param>
        /// <returns>Component's lifestyle</returns>
        /// <exception cref="AttributeRequiredException">Component doesn't marked with LifestyleAttribute</exception>
        public static EnLifestyle Lifestyle(this Type type)
        {
            var lifestyle = type.GetAttribute<LifestyleAttribute>()?.Lifestyle;

            if (lifestyle == null)
            {
                throw new AttributeRequiredException(typeof(LifestyleAttribute), type);
            }

            return lifestyle.Value;
        }

        internal static Lifestyle MapLifestyle(this EnLifestyle lifestyle)
        {
            return lifestyle switch
            {
                EnLifestyle.Transient => SimpleInjector.Lifestyle.Transient,
                EnLifestyle.Singleton => SimpleInjector.Lifestyle.Singleton,
                EnLifestyle.Scoped => SimpleInjector.Lifestyle.Scoped,
                _ => throw new NotSupportedException(lifestyle.ToString())
            };
        }

        internal static EnLifestyle MapLifestyle(this Lifestyle lifestyle)
        {
            var lifestyleType = lifestyle.GetType();

            if (lifestyleType == SimpleInjector.Lifestyle.Transient.GetType())
            {
                return EnLifestyle.Transient;
            }

            if (lifestyleType == SimpleInjector.Lifestyle.Singleton.GetType())
            {
                return EnLifestyle.Singleton;
            }

            if (typeof(ScopedLifestyle).IsAssignableFrom(lifestyleType))
            {
                return EnLifestyle.Scoped;
            }

            throw new NotSupportedException(lifestyle.ToString());
        }
    }
}