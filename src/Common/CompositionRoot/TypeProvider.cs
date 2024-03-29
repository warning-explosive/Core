namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Basics.EqualityComparers;

    [ManuallyRegisteredComponent("Is created manually and implicitly during DependencyContainer initialization")]
    internal class TypeProvider : ITypeProvider,
                                  IResolvable<ITypeProvider>
    {
        private static readonly string[] ExcludedTypes =
        {
            "<>f",
            "<>c",
            "d__",
            "<PrivateImplementationDetails>",
            "AutoGeneratedProgram",
            "Xunit",
            "System.Runtime.CompilerServices",
            "Microsoft.CodeAnalysis",
            "Coverlet.Core.Instrumentation.Tracker"
        };

        [SuppressMessage("Analysis", "SA1011", Justification = "space between square brackets and nullable symbol")]
        private static Assembly[]? _rootAssemblies;

        private readonly HashSet<string> _ourTypesCache;

        public TypeProvider(
            IReadOnlyCollection<Assembly> assemblies,
            IReadOnlyCollection<string> excludedNamespaces,
            IReadOnlyCollection<Type> additionalOurTypes)
        {
            var rootAssemblies = RootAssemblies();

            AllLoadedAssemblies = assemblies
               .Union(rootAssemblies)
               .Union(additionalOurTypes.Select(type => type.Assembly))
               .Where(assembly => !assembly.IsDynamic)
               .Distinct(new AssemblyByNameEqualityComparer())
               .ToList();

            AllLoadedTypes = AllLoadedAssemblies
               .SelectMany(assembly => assembly.GetTypes())
               .ToList();

            var isOurReference = AssembliesExtensions.IsOurReference(AllLoadedAssemblies, rootAssemblies);

            var ourLoadedAssemblies = assemblies
               .Union(rootAssemblies)
               .Where(assembly => !assembly.IsDynamic)
               .Distinct(new AssemblyByNameEqualityComparer())
               .ToList();

            OurAssemblies = ourLoadedAssemblies
               .Where(isOurReference)
               .ToList();

            OurTypes = OurAssemblies
               .SelectMany(ExtractOurTypes)
               .Where(type => !excludedNamespaces.Contains(type.Namespace, StringComparer.OrdinalIgnoreCase))
               .Union(additionalOurTypes)
               .ToList();

            _ourTypesCache = new HashSet<string>(OurTypes.Select(type => type.FullName));
        }

        #region ITypeProvider

        public IReadOnlyCollection<Assembly> AllLoadedAssemblies { get; }

        public IReadOnlyCollection<Type> AllLoadedTypes { get; }

        public IReadOnlyCollection<Assembly> OurAssemblies { get; }

        public IReadOnlyCollection<Type> OurTypes { get; }

        public bool IsOurType(Type type)
        {
            return _ourTypesCache.Contains(type.FullName);
        }

        #endregion

        private static IEnumerable<Type> ExtractOurTypes(Assembly assembly)
        {
            return assembly
               .GetTypes()
               .Where(type => type.FullName != null
                           && ExcludedTypes.All(mask => !type.FullName.Contains(mask, StringComparison.Ordinal)));
        }

        private static Assembly[] RootAssemblies()
        {
            _rootAssemblies ??= InitRootAssemblies();
            return _rootAssemblies;

            static Assembly[] InitRootAssemblies()
            {
                return new[]
                {
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Basics))),
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(AutoRegistration), nameof(AutoRegistration.Api))),
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot)))
                };
            }
        }
    }
}