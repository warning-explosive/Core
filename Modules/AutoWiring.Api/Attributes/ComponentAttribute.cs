namespace SpaceEngineers.Core.AutoWiring.Api.Attributes
{
    using System;
    using Enumerations;

    /// <summary>
    /// Component attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ComponentAttribute : Attribute
    {
        /// <summary> .ctor </summary>
        /// <param name="lifestyle">Lifestyle</param>
        /// <param name="kind">Component kind</param>
        public ComponentAttribute(EnLifestyle lifestyle, EnComponentKind kind = EnComponentKind.Regular)
        {
            Lifestyle = lifestyle;
            Kind = kind;
        }

        /// <summary>
        /// Service lifestyle
        /// </summary>
        public EnLifestyle Lifestyle { get; }

        /// <summary>
        /// Component kind
        /// </summary>
        public EnComponentKind Kind { get; }
    }
}