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
        {
            ConstructorResolutionBehavior = new ConstructorResolutionBehavior();

            ManualRegistrations = Array.Empty<IManualRegistration>();
            Overrides = Array.Empty<IComponentsOverride>();
            ExcludedAssemblies = Array.Empty<Assembly>();
            ExcludedNamespaces = Array.Empty<string>();
            AdditionalOurTypes = Array.Empty<Type>();
            ManualVerification = false;
        }

        /// <summary>
        /// Constructor resolution behavior
        /// </summary>
        public IConstructorResolutionBehavior ConstructorResolutionBehavior { get; }

        /// <summary>
        /// Manual registrations
        /// </summary>
        public IReadOnlyCollection<IManualRegistration> ManualRegistrations { get; private set; }

        /// <summary>
        /// Overrides
        /// </summary>
        public IReadOnlyCollection<IComponentsOverride> Overrides { get; private set; }

        /// <summary>
        /// Excluded assemblies
        /// Assemblies excluded from type loading
        /// These assemblies and their types will be identified as third party and won't participate in components registrations
        /// </summary>
        public IReadOnlyCollection<Assembly> ExcludedAssemblies { get; private set; }

        /// <summary>
        /// Excluded namespaces
        /// Namespaces excluded from type loading
        /// These types will be identified as third party and won't participate in components registrations
        /// </summary>
        public IReadOnlyCollection<string> ExcludedNamespaces { get; private set; }

        /// <summary>
        /// Additional our types
        /// Types that will be identified as ours and will take part in components registration
        /// </summary>
        public IReadOnlyCollection<Type> AdditionalOurTypes { get; private set; }

        /// <summary>
        /// Disables or enables automatic verification on container's creation
        /// </summary>
        public bool ManualVerification { get; private set; }

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
            ManualRegistrations = ManualRegistrations.Concat(manualRegistrations).ToList();

            return this;
        }

        /// <summary>
        /// With overrides
        /// </summary>
        /// <param name="overrides">Components overrides</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithOverrides(params IComponentsOverride[] overrides)
        {
            Overrides = Overrides.Concat(overrides).ToList();

            return this;
        }

        /// <summary>
        /// With excluded assemblies
        /// </summary>
        /// <param name="assemblies">Excluded assemblies</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithExcludedAssemblies(params Assembly[] assemblies)
        {
            ExcludedAssemblies = ExcludedAssemblies.Concat(assemblies).ToList();

            return this;
        }

        /// <summary>
        /// With excluded namespaces
        /// </summary>
        /// <param name="namespaces">Excluded namespaces</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithExcludedNamespaces(params string[] namespaces)
        {
            ExcludedNamespaces = ExcludedNamespaces.Concat(namespaces).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return this;
        }

        /// <summary>
        /// Adds types as ours that will take part in components registration
        /// </summary>
        /// <param name="additionalOurTypes">Additional our types</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithAdditionalOurTypes(params Type[] additionalOurTypes)
        {
            AdditionalOurTypes = AdditionalOurTypes.Concat(additionalOurTypes).ToHashSet();

            return this;
        }

        /// <summary>
        /// Disables or enables automatic verification on container's creation
        /// </summary>
        /// <param name="attribute">Attribute value</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithManualVerification(bool attribute)
        {
            ManualVerification = attribute;

            return this;
        }
    }
}