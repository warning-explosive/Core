namespace SpaceEngineers.Core.Basics.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Attribute which defines dependent types
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DependentAttribute : Attribute
    {
        /// <summary> .ctor </summary>
        /// <param name="dependent">Required dependent type</param>
        /// <param name="dependents">Optional dependent types</param>
        public DependentAttribute(Type dependent, params Type[] dependents)
        {
            Dependents = new List<Type>(dependents)
            {
                dependent
            };
        }

        /// <summary> .ctor </summary>
        /// <param name="dependent">Required dependent type</param>
        /// <param name="dependents">Optional dependent types</param>
        public DependentAttribute(string dependent, params string[] dependents)
        {
            Dependents = new List<string>(dependents)
                {
                    dependent
                }
               .Select(AssembliesExtensions.FindRequiredType)
               .ToArray();
        }

        /// <summary>
        /// Dependents
        /// </summary>
        public IReadOnlyCollection<Type> Dependents { get; }
    }
}