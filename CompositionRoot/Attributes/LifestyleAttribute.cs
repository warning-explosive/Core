namespace SpaceEngineers.Core.CompositionRoot.Attributes
{
    using System;
    using Enumerations;

    /// <summary>
    /// Component lifestyle attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class LifestyleAttribute : Attribute
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