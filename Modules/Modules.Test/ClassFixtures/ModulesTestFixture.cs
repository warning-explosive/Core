namespace SpaceEngineers.Core.Modules.Test.ClassFixtures
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Basics;
    using Core.SettingsManager.Extensions;
    using Registrations;

    /// <summary>
    /// ModulesTestFixture
    /// </summary>
    public sealed class ModulesTestFixture
    {
        private const string Settings = nameof(Settings);

        private static readonly ConcurrentDictionary<int, IDependencyContainer> Cache
            = new ConcurrentDictionary<int, IDependencyContainer>();

        private static readonly Lazy<IDependencyContainer> LazyDefaultDependencyContainer
            = new Lazy<IDependencyContainer>(() => CreateDependencyContainer(
                typeof(ModulesTestFixture).Assembly,
                InternalRegistrations),
                LazyThreadSafetyMode.ExecutionAndPublication);

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
        /// Default DependencyContainer
        /// </summary>
        [SuppressMessage("Analyzers", "CA1822", Justification = "xunit test fixture")]
        public IDependencyContainer DefaultDependencyContainer => LazyDefaultDependencyContainer.Value;

        /// <summary>
        /// Default DependencyContainerOptions
        /// </summary>
        [SuppressMessage("Analyzers", "CA1822", Justification = "xunit test fixture")]
        public DependencyContainerOptions DefaultOptions => CreateDependencyContainerOptions(Registrations);

        /// <summary>
        /// Registrations
        /// </summary>
        [SuppressMessage("Analyzers", "CA1822", Justification = "xunit test fixture")]
        [SuppressMessage("Analyzers", "CA1819", Justification = "generated array")]
        public IModulesTestRegistration[] Registrations => InternalRegistrations;

        private static IModulesTestRegistration[] InternalRegistrations =>
            new IModulesTestRegistration[]
            {
                new DelegatesRegistration(),
                new VersionedOpenGenericRegistration(),
                new EndpointIdentityRegistration()
            };

        /// <summary>
        /// Get DependencyContainerOptions with applied registrations
        /// </summary>
        /// <param name="excludedTypes">excluded IModulesTestRegistration types</param>
        /// <returns>DependencyContainerOptions</returns>
        [SuppressMessage("Analyzers", "CA1822", Justification = "xunit test fixture")]
        public DependencyContainerOptions GetDependencyContainerOptions(
            params Type[] excludedTypes)
        {
            return CreateDependencyContainerOptions(InternalRegistrations.Where(r => !excludedTypes.Contains(r.GetType())));
        }

        /// <summary>
        /// Setup DependencyContainer
        /// </summary>
        /// <param name="assembly">Assembly that limits assembly loading for dependency container</param>
        /// <param name="registrations">Custom registrations classes</param>
        /// <returns>IDependencyContainer</returns>
        [SuppressMessage("Analyzers", "CA1822", Justification = "xunit test fixture")]
        public IDependencyContainer GetDependencyContainer(
            Assembly assembly,
            IEnumerable<IModulesTestRegistration>? registrations = null)
        {
            var objects = new[] { assembly.GetName().FullName }
                .Concat((registrations ?? Enumerable.Empty<IModulesTestRegistration>())
                        .Select(r => r.GetType().FullName!))
                .ToList();

            var hash = DependencyContainerHash(objects);

            if (Cache.TryGetValue(hash, out var container))
            {
                return container;
            }

            container = CreateDependencyContainer(assembly, registrations);

            Cache.AddOrUpdate(hash, _ => container, (_, _) => container);

            return container;
        }

        private static int DependencyContainerHash(IEnumerable<string> objects)
        {
            return string.Join(string.Empty, objects.OrderBy(o => o)).GetHashCode(StringComparison.Ordinal);
        }

        private static IDependencyContainer CreateDependencyContainer(
            Assembly assembly,
            IEnumerable<IModulesTestRegistration>? registrations)
        {
            var options = CreateDependencyContainerOptions(registrations);

            return DependencyContainer.CreateBoundedAbove(assembly, options);
        }

        private static DependencyContainerOptions CreateDependencyContainerOptions(
            IEnumerable<IModulesTestRegistration>? registrations)
        {
            var options = new DependencyContainerOptions();
            options.OnRegistration += (_, e) =>
            {
                registrations?.Each(r => r.Register(e.Registration));
            };

            return options;
        }
    }
}