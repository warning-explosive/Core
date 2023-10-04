namespace SpaceEngineers.Core.Modules.Benchmark.Sources
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
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
        private DirectoryInfo? _settingsDirectory;

        [SuppressMessage("Analysis", "SA1011", Justification = "space between square brackets and nullable symbol")]
        private Assembly[]? _assemblies;

        private Assembly[] Assemblies => _assemblies ?? throw new InvalidOperationException(nameof(_assemblies));

        /// <summary>
        /// GlobalSetup
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            var solutionFileDirectory = SolutionExtensions.SolutionFile().Directory
                                        ?? throw new InvalidOperationException("Solution directory wasn't found");

            _settingsDirectory = solutionFileDirectory
                .StepInto(nameof(Benchmarks))
                .StepInto(AssembliesExtensions.BuildName(nameof(Modules), nameof(Benchmark)))
                .StepInto("Settings");

            _assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CrossCuttingConcerns))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(DataImport))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(DataExport))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Dynamic))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CliArgumentsParser))),

                AssembliesExtensions.FindRequiredAssembly("System.Private.CoreLib")
            };
        }

        /// <summary> CreateExactlyBounded </summary>
        /// <returns>IDependencyContainer</returns>
        [Benchmark(Description = nameof(Create))]
        public IDependencyContainer Create()
        {
            var options = new DependencyContainerOptions()
                .WithPluginAssemblies(Assemblies)
                .WithManualRegistrations(new SettingsDirectoryProviderManualRegistration(new SettingsDirectoryProvider(_settingsDirectory!)));

            return DependencyContainer.Create(options);
        }
    }
}