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
        private readonly List<IManualRegistration> _manualRegistrations;
        private readonly List<IManualRegistration> _overrides;

        /// <summary> .cctor </summary>
        public DependencyContainerOptions()
        {
            _manualRegistrations = new List<IManualRegistration>();
            _overrides = new List<IManualRegistration>();
        }

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
        public IReadOnlyCollection<IManualRegistration> ManualRegistrations => _manualRegistrations;

        /// <summary>
        /// Overrides
        /// </summary>
        public IReadOnlyCollection<IManualRegistration> Overrides => _overrides;

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

        /// <summary> With manual registration </summary>
        /// <param name="manualRegistration">IManualRegistration</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithManualRegistration(IManualRegistration manualRegistration)
        {
            _manualRegistrations.Add(manualRegistration);
            return this;
        }

        /// <summary> With override </summary>
        /// <param name="manualRegistration">IManualRegistration</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithOverride(IManualRegistration manualRegistration)
        {
            _overrides.Add(manualRegistration);
            return this;
        }
    }
}