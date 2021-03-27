namespace SpaceEngineers.Core.Test.Api.ClassFixtures
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Basics;
    using SettingsManager.Extensions;

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
                .EnsureNotNull("Project directory not found")
                .StepInto(Settings)
                .SetupFileSystemSettingsDirectory();
        }

        /// <summary>
        /// Setup DependencyContainer
        /// </summary>
        /// <param name="aboveAssembly">Assembly that limits assembly loading in dependency container</param>
        /// <param name="options">Dependency container options</param>
        /// <returns>IDependencyContainer</returns>
        [SuppressMessage("Analyzers", "CA1822", Justification = "xunit test fixture")]
        public IDependencyContainer GetDependencyContainer(
            Assembly aboveAssembly,
            DependencyContainerOptions? options = null)
        {
            options ??= new DependencyContainerOptions();

            var hash = DependencyContainerHash(aboveAssembly, options);

            if (Cache.TryGetValue(hash, out var container))
            {
                return container;
            }

            container = DependencyContainer.CreateBoundedAbove(aboveAssembly, options);

            Cache.AddOrUpdate(hash, _ => container, (_, _) => container);

            return container;
        }

        private static int DependencyContainerHash(
            Assembly aboveAssembly,
            DependencyContainerOptions options)
        {
            return HashCode.Combine(aboveAssembly, options);
        }
    }
}