namespace SpaceEngineers.Core.AutoRegistration.Api.Attributes
{
    using System;
    using Enumerations;

    /// <summary>
    /// Defines application component that should be automatically wired into dependency container
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ComponentAttribute : Attribute
    {
        /// <summary> .ctor </summary>
        /// <param name="lifestyle">Lifestyle</param>
        public ComponentAttribute(EnLifestyle lifestyle)
        {
            Lifestyle = lifestyle;
        }

        /// <summary>
        /// Service lifestyle
        /// </summary>
        public EnLifestyle Lifestyle { get; }
    }
}