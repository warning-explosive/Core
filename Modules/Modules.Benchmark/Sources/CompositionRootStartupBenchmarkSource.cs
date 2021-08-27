namespace SpaceEngineers.Core.Modules.Benchmark.Sources
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Basics;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Engines;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using Core.Test.Api;
    using CrossCuttingConcerns.Settings;
    using Test.Registrations;

    /// <summary>
    /// Creates DependencyContainer and measures cold start time
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart)]
    public class CompositionRootStartupBenchmarkSource
    {
        [SuppressMessage("Analysis", "SA1011", Justification = "space between square brackets and nullable symbol")]
        private Assembly[]? _assemblies;
        private DependencyContainerOptions? _options;

        private Assembly[] Assemblies => _assemblies.EnsureNotNull(nameof(_assemblies));

        private DependencyContainerOptions Options => _options.EnsureNotNull(nameof(_options));

        /// <summary>
        /// GlobalSetup
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            SolutionExtensions
                .SolutionFile()
                .Directory
                .EnsureNotNull("Solution directory not found")
                .StepInto(nameof(Modules))
                .StepInto(AssembliesExtensions.BuildName(nameof(Modules), nameof(Modules.Benchmark)))
                .StepInto("Settings")
                .SetupFileSystemSettingsDirectory();

            _assemblies = ModulesTestFixtureExtensions.ModulesAssemblies;
            _options = ModulesTestFixtureExtensions.ModulesOptions;
        }

        /// <summary> CreateBoundedAbove </summary>
        /// <returns>IDependencyContainer</returns>
        [Benchmark(Description = nameof(CreateBoundedAbove), Baseline = true)]
        public IDependencyContainer CreateBoundedAbove()
        {
            return DependencyContainer.CreateBoundedAbove(
                Options,
                Options.UseGenericContainer(),
                Assemblies);
        }
    }
}