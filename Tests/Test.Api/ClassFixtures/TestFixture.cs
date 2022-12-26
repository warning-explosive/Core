namespace SpaceEngineers.Core.Test.Api.ClassFixtures
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using CompositionRoot;
    using CompositionRoot.Registration;
    using Internals;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;

    /// <summary>
    /// TestFixture
    /// </summary>
    public sealed class TestFixture : IModulesTestFixture
    {
        private static readonly ConcurrentDictionary<int, IDependencyContainer> Cache
            = new ConcurrentDictionary<int, IDependencyContainer>();

        /// <summary> .cctor </summary>
        public TestFixture()
        {
        }

        /// <inheritdoc />
        public IHostBuilder CreateHostBuilder()
        {
            return Host
               .CreateDefaultBuilder()
               .ConfigureLogging(context =>
               {
                   context.AddConsole();
                   context.SetMinimumLevel(LogLevel.Trace);
               });
        }

        /// <inheritdoc />
        public IHostBuilder CreateHostBuilder(ITestOutputHelper testOutputHelper)
        {
            return Host
               .CreateDefaultBuilder()
               .ConfigureLogging(context =>
               {
                   context.AddProvider(new XUnitLoggerProvider(testOutputHelper));
                   context.SetMinimumLevel(LogLevel.Trace);
               });
        }

        /// <inheritdoc />
        public ILogger CreateLogger(ITestOutputHelper testOutputHelper)
        {
            return new XUnitConsoleLogger(testOutputHelper);
        }

        /// <inheritdoc />
        public IManualRegistration DelegateRegistration(Action<IManualRegistrationsContainer> registrationAction)
        {
            return new ManualDelegateRegistration(registrationAction);
        }

        /// <inheritdoc />
        public IComponentsOverride DelegateOverride(Action<IRegisterComponentsOverrideContainer> overrideAction)
        {
            return new DelegateComponentsOverride(overrideAction);
        }

        /// <inheritdoc />
        public IDependencyContainer BoundedAboveContainer(
            ITestOutputHelper output,
            DependencyContainerOptions options,
            params Assembly[] aboveAssemblies)
        {
            return CreateDependencyContainer(
                options,
                DependencyContainer.CreateBoundedAbove,
                aboveAssemblies);
        }

        /// <inheritdoc />
        public IDependencyContainer ExactlyBoundedContainer(
            ITestOutputHelper output,
            DependencyContainerOptions options,
            params Assembly[] exactAssemblies)
        {
            return CreateDependencyContainer(
                options,
                DependencyContainer.CreateExactlyBounded,
                exactAssemblies);
        }

        /// <inheritdoc />
        public IDependencyContainer Container(
            ITestOutputHelper output,
            DependencyContainerOptions options)
        {
            return CreateDependencyContainer(
                options,
                (containerOptions, _) => DependencyContainer.Create(containerOptions));
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