namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abstractions;

    /// <summary>
    /// DependencyContainer creation options
    /// </summary>
    public class DependencyContainerOptions
    {
        /// <summary>
        /// Excluded assemblies
        /// Assemblies excluded from type loading
        /// These assemblies and their types will be identified as third party and won't participate in the container registrations
        /// </summary>
        public IReadOnlyCollection<Assembly> ExcludedAssemblies { get; set; } = Array.Empty<Assembly>();

        /// <summary>
        /// Excluded namespaces
        /// Namespaces excluded from type loading
        /// These types will be identified as third party and won't participate in the container registrations
        /// </summary>
        public IReadOnlyCollection<string> ExcludedNamespaces { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Manual registrations
        /// </summary>
        public IReadOnlyCollection<IManualRegistration> ManualRegistrations { get; set; } = Array.Empty<IManualRegistration>();

        /// <summary>
        /// Overrides
        /// </summary>
        public IReadOnlyCollection<IManualRegistration> Overrides { get; set; } = Array.Empty<IManualRegistration>();

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                CombineHashCode(ExcludedAssemblies),
                CombineHashCode(ExcludedNamespaces),
                CombineHashCode(ManualRegistrations),
                CombineHashCode(Overrides));

            static int CombineHashCode<T>(IReadOnlyCollection<T> source)
            {
                return source.Any()
                    ? source.Aggregate(int.MaxValue, HashCode.Combine)
                    : int.MaxValue;
            }
        }
    }
}