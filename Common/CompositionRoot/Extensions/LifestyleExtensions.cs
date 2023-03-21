namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using AutoRegistration.Api.Enumerations;
    using SimpleInjector;

    internal static class LifestyleExtensions
    {
        internal static Lifestyle MapLifestyle(this EnLifestyle lifestyle)
        {
            return lifestyle switch
            {
                EnLifestyle.Transient => Lifestyle.Transient,
                EnLifestyle.Singleton => Lifestyle.Singleton,
                EnLifestyle.Scoped => Lifestyle.Scoped,
                _ => throw new NotSupportedException(lifestyle.ToString())
            };
        }

        internal static EnLifestyle MapLifestyle(this Lifestyle lifestyle)
        {
            var lifestyleType = lifestyle.GetType();

            if (lifestyleType == Lifestyle.Transient.GetType())
            {
                return EnLifestyle.Transient;
            }

            if (lifestyleType == Lifestyle.Singleton.GetType())
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