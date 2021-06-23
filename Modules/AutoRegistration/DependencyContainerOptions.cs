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
        /// <summary> .cctor </summary>
        public DependencyContainerOptions()
        {
            ManualRegistrations = new List<IManualRegistration>();
            Overrides = new List<IManualRegistration>();

            ExcludedAssemblies = Array.Empty<Assembly>();
            ExcludedNamespaces = Array.Empty<string>();
        }

        /// <summary>
        /// Excluded assemblies
        /// Assemblies excluded from type loading
        /// These assemblies and their types will be identified as third party and won't participate in the container registrations
        /// </summary>
        public IReadOnlyCollection<Assembly> ExcludedAssemblies { get; private set; }

        /// <summary>
        /// Excluded namespaces
        /// Namespaces excluded from type loading
        /// These types will be identified as third party and won't participate in the container registrations
        /// </summary>
        public IReadOnlyCollection<string> ExcludedNamespaces { get; private set; }

        /// <summary>
        /// Manual registrations
        /// </summary>
        public IReadOnlyCollection<IManualRegistration> ManualRegistrations { get; private set; }

        /// <summary>
        /// Overrides
        /// </summary>
        public IReadOnlyCollection<IManualRegistration> Overrides { get; private set; }

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

        /// <summary>
        /// With manual registrations
        /// </summary>
        /// <param name="manualRegistrations">Manual registrations</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithManualRegistration(params IManualRegistration[] manualRegistrations)
        {
            return new DependencyContainerOptions()
            {
                ManualRegistrations = ManualRegistrations.Concat(manualRegistrations).ToList(),
                Overrides = Overrides,
                ExcludedAssemblies = ExcludedAssemblies,
                ExcludedNamespaces = ExcludedNamespaces,
            };
        }

        /// <summary>
        /// With overrides
        /// </summary>
        /// <param name="overrides">Overrides</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithOverride(params IManualRegistration[] overrides)
        {
            return new DependencyContainerOptions()
            {
                ManualRegistrations = ManualRegistrations,
                Overrides = Overrides.Concat(overrides).ToList(),
                ExcludedAssemblies = ExcludedAssemblies,
                ExcludedNamespaces = ExcludedNamespaces,
            };
        }

        /// <summary>
        /// With excluded assemblies
        /// </summary>
        /// <param name="assemblies">Excluded assemblies</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithExcludedAssembly(params Assembly[] assemblies)
        {
            return new DependencyContainerOptions()
            {
                ManualRegistrations = ManualRegistrations,
                Overrides = Overrides,
                ExcludedAssemblies = ExcludedAssemblies.Concat(assemblies).ToList(),
                ExcludedNamespaces = ExcludedNamespaces,
            };
        }

        /// <summary>
        /// With excluded namespaces
        /// </summary>
        /// <param name="namespaces">Excluded namespaces</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithExcludedNamespace(params string[] namespaces)
        {
            return new DependencyContainerOptions()
            {
                ManualRegistrations = ManualRegistrations,
                Overrides = Overrides,
                ExcludedAssemblies = ExcludedAssemblies,
                ExcludedNamespaces = ExcludedNamespaces.Concat(namespaces).ToList(),
            };
        }
    }
}