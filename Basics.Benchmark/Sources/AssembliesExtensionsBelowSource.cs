namespace Basics.Benchmark.Sources
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Engines;
    using SpaceEngineers.Core.Basics;

    /// <summary>
    /// AssembliesExtensions.Below(assembly) benchmark source
    /// </summary>
    [SimpleJob(RunStrategy.Throughput)]
    public class AssembliesExtensionsBelowSource
    {
        [SuppressMessage("Analysis", "SA1011", Justification = "space between square brackets and nullable symbol")]
        private Assembly[]? _allAssemblies;
        private Assembly? _belowAssembly;

        private Assembly[] AllAssemblies => _allAssemblies.EnsureNotNull(nameof(_allAssemblies));

        private Assembly BelowAssembly => _belowAssembly.EnsureNotNull(nameof(_belowAssembly));

        /// <summary>
        /// GlobalSetup
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            _allAssemblies = AssembliesExtensions.AllAssembliesFromCurrentDomain();
            _belowAssembly = GetType().Assembly;
        }

        /// <summary> DeepCopyBySerialization </summary>
        /// <returns>Copy</returns>
        [Benchmark(Description = nameof(Below), Baseline = true)]
        public Assembly[] Below() => AllAssemblies.Below(BelowAssembly);
    }
}