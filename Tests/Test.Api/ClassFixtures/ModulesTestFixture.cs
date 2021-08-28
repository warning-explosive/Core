namespace SpaceEngineers.Core.Test.Api.ClassFixtures
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using CrossCuttingConcerns.Api.Extensions;
    using Internals;

    /// <summary>
    /// ModulesTestFixture
    /// </summary>
    public sealed class ModulesTestFixture : IModulesTestFixture
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

        /// <inheritdoc />
        public IManualRegistration DelegateRegistration(Action<IManualRegistrationsContainer> registrationAction)
        {
            return new ManualDelegateRegistration(registrationAction);
        }

        /// <inheritdoc />
        public IDependencyContainer BoundedAboveContainer(DependencyContainerOptions options, params Assembly[] aboveAssemblies)
        {
            return CreateDependencyContainer(
                options,
                (containerOptions, assemblies) => DependencyContainer.CreateBoundedAbove(containerOptions, containerOptions.UseGenericContainer(), assemblies),
                aboveAssemblies);
        }

        /// <inheritdoc />
        public IDependencyContainer ExactlyBoundedContainer(DependencyContainerOptions options, params Assembly[] exactAssemblies)
        {
            return CreateDependencyContainer(
                options,
                (containerOptions, assemblies) => DependencyContainer.CreateExactlyBounded(containerOptions, containerOptions.UseGenericContainer(), assemblies),
                exactAssemblies);
        }

        /// <inheritdoc />
        public IDependencyContainer CreateContainer(DependencyContainerOptions options)
        {
            return CreateDependencyContainer(
                options,
                (containerOptions, _) => DependencyContainer.Create(containerOptions, containerOptions.UseGenericContainer()));
        }

        private static IDependencyContainer CreateDependencyContainer(
            DependencyContainerOptions options,
            Func<DependencyContainerOptions, Assembly[], IDependencyContainer> factory,
            params Assembly[] assemblies)
        {
            var hash = DependencyContainerHash(options, assemblies);

            if (Cache.TryGetValue(hash, out var container))
            {
                return container;
            }

            container = factory(options, assemblies);

            return Cache.AddOrUpdate(hash, _ => container, (_, _) => container);
        }

        private static int DependencyContainerHash(
            DependencyContainerOptions options,
            params Assembly[] aboveAssembly)
        {
            return HashCode.Combine(aboveAssembly.Aggregate(int.MaxValue, HashCode.Combine), options);
        }
    }
}