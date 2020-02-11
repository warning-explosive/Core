namespace SpaceEngineers.Core.AutoWiringApi.Attributes
{
    using System;
    using Basics.Attributes;
    using Enumerations;

    /// <summary>
    /// Component lifestyle attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class LifestyleAttribute : BaseAttribute
    {
        /// <summary> .ctor </summary>
        /// <param name="lifestyle">Lifestyle</param>
        public LifestyleAttribute(EnLifestyle lifestyle)
        {
            Lifestyle = lifestyle;
        }

        /// <summary>
        /// Service lifestyle
        /// </summary>
        public EnLifestyle Lifestyle { get; }
    }
}