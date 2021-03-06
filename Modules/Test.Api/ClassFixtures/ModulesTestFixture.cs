namespace SpaceEngineers.Core.Test.Api.ClassFixtures
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Basics;
    using Internals;
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
        /// Generates IManualRegistration object with specified delegate
        /// </summary>
        /// <param name="registrationAction">Action with IRegistrationContainer instance</param>
        /// <returns>IManualRegistration instance</returns>
        public IManualRegistration DelegateRegistration(Action<IManualRegistrationsContainer> registrationAction)
        {
            return new ManualDelegateRegistration(registrationAction);
        }

        /// <summary>
        /// Setup bounded above dependency container
        /// </summary>
        /// <param name="options">Dependency container options</param>
        /// <param name="aboveAssemblies">Assemblies that limits assembly loading in dependency container</param>
        /// <returns>IDependencyContainer</returns>
        public IDependencyContainer BoundedAboveContainer(
            DependencyContainerOptions options,
            params Assembly[] aboveAssemblies)
        {
            return CreateDependencyContainer(options, DependencyContainer.CreateBoundedAbove, aboveAssemblies);
        }

        /// <summary>
        /// Setup exactly bounded dependency container
        /// </summary>
        /// <param name="options">Dependency container options</param>
        /// <param name="exactAssemblies">Assemblies that limits assembly loading in dependency container</param>
        /// <returns>IDependencyContainer</returns>
        public IDependencyContainer ExactlyBoundedContainer(
            DependencyContainerOptions options,
            params Assembly[] exactAssemblies)
        {
            return CreateDependencyContainer(options, DependencyContainer.CreateExactlyBounded, exactAssemblies);
        }

        /// <summary>
        /// Setup dependency container without assembly limitations
        /// </summary>
        /// <param name="options">Dependency container options</param>
        /// <returns>IDependencyContainer</returns>
        public IDependencyContainer CreateContainer(DependencyContainerOptions options)
        {
            return CreateDependencyContainer(options, (containerOptions, _) => DependencyContainer.Create(containerOptions));
        }

        private IDependencyContainer CreateDependencyContainer(
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