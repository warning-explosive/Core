namespace SpaceEngineers.Core.Modules.Test.ClassFixtures
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Basics;
    using Core.SettingsManager.Extensions;

    /// <summary>
    /// ModulesTestFixture
    /// </summary>
    public sealed class ModulesTestFixture
    {
        private const string Settings = nameof(Settings);

        private static readonly ConcurrentDictionary<int, IDependencyContainer> Cache
            = new ConcurrentDictionary<int, IDependencyContainer>();

        /// <summary> .cctor </summary>
        public ModulesTestFixture()
        {
            SolutionExtensions
                .ProjectFile()
                .Directory
                .EnsureNotNull($"Project directory {nameof(Modules)}.{nameof(Modules.Test)} not found")
                .StepInto(Settings)
                .SetupFileSystemSettingsDirectory();
        }

        /// <summary>
        /// Setup DependencyContainer
        /// </summary>
        /// <param name="aboveAssembly">Assembly that limits assembly loading in dependency container</param>
        /// <param name="excludedAssemblies">Assemblies excluded from loading in dependency container</param>
        /// <param name="registrations">Custom registrations classes</param>
        /// <returns>IDependencyContainer</returns>
        [SuppressMessage("Analyzers", "CA1822", Justification = "xunit test fixture")]
        public IDependencyContainer GetDependencyContainer(
            Assembly aboveAssembly,
            Assembly[] excludedAssemblies,
            params IManualRegistration[] registrations)
        {
            var hash = DependencyContainerHash(aboveAssembly, excludedAssemblies, registrations);

            if (Cache.TryGetValue(hash, out var container))
            {
                return container;
            }

            container = CreateDependencyContainer(aboveAssembly, excludedAssemblies, registrations);

            Cache.AddOrUpdate(hash, _ => container, (_, _) => container);

            return container;
        }

        private static int DependencyContainerHash(
            Assembly aboveAssembly,
            Assembly[] excludedAssemblies,
            params IManualRegistration[] registrations)
        {
            var aboveAssemblyKey = aboveAssembly.GetName().FullName;
            var excludedAssembliesKey = string.Join(string.Empty, excludedAssemblies.Select(asm => asm.GetName().FullName).OrderBy(name => name));
            var registrationsKey = string.Join(string.Empty, registrations.Select(r => r.GetType().FullName!).OrderBy(name => name));

            return string
                .Join(string.Empty, aboveAssemblyKey, excludedAssembliesKey, registrationsKey)
                .GetHashCode(StringComparison.Ordinal);
        }

        private static IDependencyContainer CreateDependencyContainer(
            Assembly assembly,
            Assembly[] excludedAssemblies,
            IManualRegistration[] registrations)
        {
            var options = CreateDependencyContainerOptions(excludedAssemblies, registrations);

            return DependencyContainer.CreateBoundedAbove(assembly, options);
        }

        private static DependencyContainerOptions CreateDependencyContainerOptions(
            Assembly[] excludedAssemblies,
            IManualRegistration[] registrations)
        {
            return new DependencyContainerOptions
            {
                ExcludedAssemblies = excludedAssemblies,
                ManualRegistrations = registrations
            };
        }
    }
}