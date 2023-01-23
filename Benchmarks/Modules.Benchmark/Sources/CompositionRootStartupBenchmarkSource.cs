namespace SpaceEngineers.Core.Modules.Benchmark.Sources
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Basics;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Engines;
    using CompositionRoot;
    using CrossCuttingConcerns.Settings;

    /// <summary>
    /// Creates DependencyContainer and measures cold start time
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart)]
    public class CompositionRootStartupBenchmarkSource
    {
        [SuppressMessage("Analysis", "SA1011", Justification = "space between square brackets and nullable symbol")]
        private Assembly[]? _assemblies;

        private Assembly[] Assemblies => _assemblies.EnsureNotNull(nameof(_assemblies));

        /// <summary>
        /// GlobalSetup
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            SolutionExtensions
                .SolutionFile()
                .Directory
                .EnsureNotNull("Solution directory wasn't found")
                .StepInto(nameof(Benchmarks))
                .StepInto(AssembliesExtensions.BuildName(nameof(Modules), nameof(Benchmark)))
                .StepInto("Settings")
                .SetupSettingsDirectory();

            _assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Basics))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(AutoRegistration), nameof(AutoRegistration.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CrossCuttingConcerns))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(DataImport))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(DataExport))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Dynamic))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CliArgumentsParser))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(PathResolver))),

                AssembliesExtensions.FindRequiredAssembly("System.Private.CoreLib")
            };
        }

        /// <summary> CreateExactlyBounded </summary>
        /// <returns>IDependencyContainer</returns>
        [Benchmark(Description = nameof(CreateExactlyBounded))]
        public IDependencyContainer CreateExactlyBounded()
        {
            return DependencyContainer.CreateExactlyBounded(new DependencyContainerOptions(), Assemblies);
        }

        /// <summary> CreateBoundedAbove </summary>
        /// <returns>IDependencyContainer</returns>
        [Benchmark(Description = nameof(CreateBoundedAbove), Baseline = true)]
        public IDependencyContainer CreateBoundedAbove()
        {
            return DependencyContainer.CreateBoundedAbove(new DependencyContainerOptions(), Assemblies);
        }
    }
}