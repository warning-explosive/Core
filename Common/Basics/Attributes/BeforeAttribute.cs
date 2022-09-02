namespace SpaceEngineers.Core.Basics.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Attribute which defines priority of types
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class BeforeAttribute : Attribute
    {
        /// <summary> .ctor </summary>
        /// <param name="type">Required type</param>
        /// <param name="types">Optional types</param>
        public BeforeAttribute(Type type, params Type[] types)
        {
            Types = new List<Type>(types) { type };
        }

        /// <summary> .ctor </summary>
        /// <param name="type">Required type</param>
        /// <param name="types">Optional types</param>
        public BeforeAttribute(string type, params string[] types)
        {
            Types = new List<string>(types) { type }
               .Select(AssembliesExtensions.FindRequiredType)
               .ToArray();
        }

        /// <summary>
        /// Types
        /// </summary>
        public IReadOnlyCollection<Type> Types { get; }
    }
}