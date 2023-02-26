namespace SpaceEngineers.Core.Basics.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Attribute which defines priority of types
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class AfterAttribute : Attribute
    {
        /// <summary> .ctor </summary>
        /// <param name="type">Required type</param>
        /// <param name="types">Optional types</param>
        public AfterAttribute(Type type, params Type[] types)
        {
            Types = new List<Type>(types) { type };
        }

        /// <summary> .ctor </summary>
        /// <param name="type">Required type</param>
        /// <param name="types">Optional types</param>
        public AfterAttribute(string type, params string[] types)
        {
            Types = new List<string>(types) { type }
               .Select(static type => TypeExtensions.FindType(type))
               .ToArray();
        }

        /// <summary>
        /// Types
        /// </summary>
        public IReadOnlyCollection<Type> Types { get; }
    }
}