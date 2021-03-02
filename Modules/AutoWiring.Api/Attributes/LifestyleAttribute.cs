namespace SpaceEngineers.Core.AutoWiring.Api.Attributes
{
    using System;
    using Enumerations;

    /// <summary>
    /// Component lifestyle attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class LifestyleAttribute : Attribute
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