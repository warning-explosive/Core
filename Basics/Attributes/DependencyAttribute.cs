namespace SpaceEngineers.Core.Basics.Attributes
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Attribute which defines dependencies of type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DependencyAttribute : Attribute
    {
        /// <summary> .ctor </summary>
        /// <param name="dependency">Required dependency</param>
        /// <param name="dependencies">Optional dependencies</param>
        public DependencyAttribute(Type dependency, params Type[] dependencies)
        {
            Dependencies = new List<Type>(dependencies)
                           {
                               dependency
                           };
        }

        /// <summary>
        /// Dependencies
        /// </summary>
        public IReadOnlyCollection<Type> Dependencies { get; }
    }
}