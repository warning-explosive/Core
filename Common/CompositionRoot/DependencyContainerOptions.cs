namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Registration;

    /// <summary>
    /// DependencyContainer creation options
    /// </summary>
    [ManuallyRegisteredComponent("Is created manually during DependencyContainer initialization")]
    public class DependencyContainerOptions : IResolvable<DependencyContainerOptions>
    {
        private readonly HashSet<Assembly> _assemblies;

        /// <summary> .cctor </summary>
        public DependencyContainerOptions()
        {
            _assemblies = new HashSet<Assembly>
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Basics))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(AutoRegistration), nameof(AutoRegistration.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot)))
            };

            ConstructorResolutionBehavior = new ConstructorResolutionBehavior();

            ManualRegistrations = Array.Empty<IManualRegistration>();
            Overrides = Array.Empty<IComponentsOverride>();
            ExcludedNamespaces = Array.Empty<string>();
            AdditionalOurTypes = Array.Empty<Type>();
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
        /// Assemblies which will be identified as ours and will take part in components registration
        /// </summary>
        public IReadOnlyCollection<Assembly> Assemblies => _assemblies;

        /// <summary>
        /// Namespaces which will be identified as third party and won't participate in components registrations
        /// </summary>
        public IReadOnlyCollection<string> ExcludedNamespaces { get; private set; }

        /// <summary>
        /// Types which will be identified as ours and will take part in components registration
        /// </summary>
        public IReadOnlyCollection<Type> AdditionalOurTypes { get; private set; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                CombineHashCode(ManualRegistrations),
                CombineHashCode(Overrides),
                CombineHashCode(Assemblies),
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
        /// With plugin assemblies
        /// </summary>
        /// <param name="assemblies">Assemblies</param>
        /// <returns>DependencyContainerOptions</returns>
        public DependencyContainerOptions WithPluginAssemblies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                if (_assemblies.Contains(assembly))
                {
                    throw new InvalidOperationException($"Assembly '{assembly}' already added as plugin assembly");
                }

                _assemblies.Add(assembly);
            }

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
    }
}