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
        /// <param name="manualRegistration">Required manual registration</param>
        /// <param name="manualRegistrations">Optional manual registrations</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithManualRegistrations(IManualRegistration manualRegistration, params IManualRegistration[] manualRegistrations)
        {
            return new DependencyContainerOptions()
            {
                ManualRegistrations = ManualRegistrations.Concat(new[] { manualRegistration }).Concat(manualRegistrations).ToList(),
                Overrides = Overrides,
                ExcludedAssemblies = ExcludedAssemblies,
                ExcludedNamespaces = ExcludedNamespaces,
            };
        }

        /// <summary>
        /// With overrides
        /// </summary>
        /// <param name="override">Required override</param>
        /// <param name="overrides">Optional overrides</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithOverrides(IManualRegistration @override, params IManualRegistration[] overrides)
        {
            return new DependencyContainerOptions()
            {
                ManualRegistrations = ManualRegistrations,
                Overrides = Overrides.Concat(new[] { @override }).Concat(overrides).ToList(),
                ExcludedAssemblies = ExcludedAssemblies,
                ExcludedNamespaces = ExcludedNamespaces,
            };
        }

        /// <summary>
        /// With excluded assemblies
        /// </summary>
        /// <param name="assembly">Required excluded assembly</param>
        /// <param name="assemblies">Optional excluded assemblies</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithExcludedAssemblies(Assembly assembly, params Assembly[] assemblies)
        {
            return new DependencyContainerOptions()
            {
                ManualRegistrations = ManualRegistrations,
                Overrides = Overrides,
                ExcludedAssemblies = ExcludedAssemblies.Concat(new[] { assembly }).Concat(assemblies).ToList(),
                ExcludedNamespaces = ExcludedNamespaces,
            };
        }

        /// <summary>
        /// With excluded namespaces
        /// </summary>
        /// <param name="namespace">Required excluded namespace</param>
        /// <param name="namespaces">Optional excluded namespaces</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithExcludedNamespaces(string @namespace, params string[] namespaces)
        {
            return new DependencyContainerOptions()
            {
                ManualRegistrations = ManualRegistrations,
                Overrides = Overrides,
                ExcludedAssemblies = ExcludedAssemblies,
                ExcludedNamespaces = ExcludedNamespaces.Concat(new[] { @namespace }).Concat(namespaces).ToList(),
            };
        }
    }
}