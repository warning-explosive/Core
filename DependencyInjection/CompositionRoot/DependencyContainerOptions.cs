namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Api.Abstractions;
    using Api.Abstractions.Registration;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Implementations;

    /// <summary>
    /// DependencyContainer creation options
    /// </summary>
    [ManuallyRegisteredComponent("Is created manually during DependencyContainer initialization")]
    public class DependencyContainerOptions : IResolvable
    {
        /// <summary> .cctor </summary>
        public DependencyContainerOptions()
            : this(Array.Empty<IManualRegistration>(),
                Array.Empty<IComponentsOverride>(),
                Array.Empty<Assembly>(),
                Array.Empty<string>(),
                Array.Empty<Type>(),
                false)
        {
        }

        private DependencyContainerOptions(
            IReadOnlyCollection<IManualRegistration> manualRegistrations,
            IReadOnlyCollection<IComponentsOverride> overrides,
            IReadOnlyCollection<Assembly> excludedAssemblies,
            IReadOnlyCollection<string> excludedNamespaces,
            IReadOnlyCollection<Type> additionalOurTypes,
            bool manualVerification)
        {
            ConstructorResolutionBehavior = new ConstructorResolutionBehavior();

            ManualRegistrations = manualRegistrations;
            Overrides = overrides;
            ExcludedAssemblies = excludedAssemblies;
            ExcludedNamespaces = excludedNamespaces;
            AdditionalOurTypes = additionalOurTypes;
            ManualVerification = manualVerification;
        }

        /// <summary>
        /// Constructor resolution behavior
        /// </summary>
        public IConstructorResolutionBehavior ConstructorResolutionBehavior { get; }

        /// <summary>
        /// Manual registrations
        /// </summary>
        public IReadOnlyCollection<IManualRegistration> ManualRegistrations { get; init; }

        /// <summary>
        /// Overrides
        /// </summary>
        public IReadOnlyCollection<IComponentsOverride> Overrides { get; init; }

        /// <summary>
        /// Excluded assemblies
        /// Assemblies excluded from type loading
        /// These assemblies and their types will be identified as third party and won't participate in components registrations
        /// </summary>
        public IReadOnlyCollection<Assembly> ExcludedAssemblies { get; init; }

        /// <summary>
        /// Excluded namespaces
        /// Namespaces excluded from type loading
        /// These types will be identified as third party and won't participate in components registrations
        /// </summary>
        public IReadOnlyCollection<string> ExcludedNamespaces { get; init; }

        /// <summary>
        /// Additional our types
        /// Types that will be identified as ours and will take part in components registration
        /// </summary>
        public IReadOnlyCollection<Type> AdditionalOurTypes { get; init; }

        /// <summary>
        /// Disables or enables automatic verification on container's creation
        /// </summary>
        public bool ManualVerification { get; init; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                CombineHashCode(ManualRegistrations),
                CombineHashCode(Overrides),
                CombineHashCode(ExcludedAssemblies),
                CombineHashCode(ExcludedNamespaces),
                CombineHashCode(AdditionalOurTypes));

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
        public DependencyContainerOptions WithManualRegistrations(params IManualRegistration[] manualRegistrations)
        {
            return !manualRegistrations.Any()
                ? this
                : new DependencyContainerOptions(
                    ManualRegistrations.Concat(manualRegistrations).ToList(),
                    Overrides,
                    ExcludedAssemblies,
                    ExcludedNamespaces,
                    AdditionalOurTypes,
                    ManualVerification);
        }

        /// <summary>
        /// With overrides
        /// </summary>
        /// <param name="overrides">Components overrides</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithOverrides(params IComponentsOverride[] overrides)
        {
            return !overrides.Any()
                ? this
                : new DependencyContainerOptions(
                    ManualRegistrations,
                    Overrides.Concat(overrides).ToList(),
                    ExcludedAssemblies,
                    ExcludedNamespaces,
                    AdditionalOurTypes,
                    ManualVerification);
        }

        /// <summary>
        /// With excluded assemblies
        /// </summary>
        /// <param name="assemblies">Excluded assemblies</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithExcludedAssemblies(params Assembly[] assemblies)
        {
            return !assemblies.Any()
                ? this
                : new DependencyContainerOptions(
                    ManualRegistrations,
                    Overrides,
                    ExcludedAssemblies.Concat(assemblies).ToList(),
                    ExcludedNamespaces,
                    AdditionalOurTypes,
                    ManualVerification);
        }

        /// <summary>
        /// With excluded namespaces
        /// </summary>
        /// <param name="namespaces">Excluded namespaces</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithExcludedNamespaces(params string[] namespaces)
        {
            return !namespaces.Any()
                ? this
                : new DependencyContainerOptions(
                    ManualRegistrations,
                    Overrides,
                    ExcludedAssemblies,
                    ExcludedNamespaces.Concat(namespaces).ToHashSet(StringComparer.OrdinalIgnoreCase),
                    AdditionalOurTypes,
                    ManualVerification);
        }

        /// <summary>
        /// Adds types as ours that will take part in components registration
        /// </summary>
        /// <param name="additionalOurTypes">Additional our types</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithAdditionalOurTypes(params Type[] additionalOurTypes)
        {
            return !additionalOurTypes.Any()
                ? this
                : new DependencyContainerOptions(
                    ManualRegistrations,
                    Overrides,
                    ExcludedAssemblies,
                    ExcludedNamespaces,
                    AdditionalOurTypes.Concat(additionalOurTypes).ToHashSet(),
                    ManualVerification);
        }

        /// <summary>
        /// Disables or enables automatic verification on container's creation
        /// </summary>
        /// <param name="attribute">Attribute value</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithManualVerification(bool attribute)
        {
            return new DependencyContainerOptions(
                ManualRegistrations,
                Overrides,
                ExcludedAssemblies,
                ExcludedNamespaces,
                AdditionalOurTypes,
                attribute);
        }
    }
}