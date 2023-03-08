namespace SpaceEngineers.Core.Modules.Benchmark.Sources
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Basics;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Engines;

    /// <summary>
    /// AssembliesExtensions.Below(assembly) benchmark source
    /// </summary>
    [SimpleJob(RunStrategy.Throughput)]
    public class AssembliesExtensionsBelowBenchmarkSource
    {
        [SuppressMessage("Analysis", "SA1011", Justification = "space between square brackets and nullable symbol")]
        private Assembly[]? _allAssemblies;
        private Assembly? _belowAssembly;

        private Assembly[] AllAssemblies => _allAssemblies ?? throw new InvalidOperationException(nameof(_allAssemblies));

        private Assembly BelowAssembly => _belowAssembly ?? throw new InvalidOperationException(nameof(_belowAssembly));

        /// <summary>
        /// GlobalSetup
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            _allAssemblies = AssembliesExtensions.AllAssembliesFromCurrentDomain();
            _belowAssembly = GetType().Assembly;
        }

        /// <summary> AssembliesExtensions.Below </summary>
        /// <returns>Below assemblies</returns>
        [Benchmark(Description = nameof(Below), Baseline = true)]
        public Assembly[] Below() => AllAssemblies.Below(BelowAssembly);
    }
}