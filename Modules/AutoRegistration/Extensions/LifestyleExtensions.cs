namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System;
    using System.Reflection;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Exceptions;
    using SimpleInjector;

    internal static class LifestyleExtensions
    {
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

        internal static EnLifestyle Lifestyle(this Type type)
        {
            var lifestyle = type.GetCustomAttribute<LifestyleAttribute>()?.Lifestyle;

            if (lifestyle == null)
            {
                throw new AttributeRequiredException(typeof(LifestyleAttribute), type);
            }

            return lifestyle.Value;
        }
    }
}