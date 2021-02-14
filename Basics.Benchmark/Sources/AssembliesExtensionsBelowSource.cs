namespace Basics.Benchmark.Sources
{
    using System.IO;
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
        private readonly Assembly[] _allAssemblies;
        private readonly Assembly _belowAssembly;

        /// <summary> .cctor </summary>
        public AssembliesExtensionsBelowSource()
        {
            AssembliesExtensions.WarmUpAppDomain(SearchOption.TopDirectoryOnly);
            _allAssemblies = AssembliesExtensions.AllFromCurrentDomain();
            _belowAssembly = GetType().Assembly;
        }

        /// <summary> DeepCopyBySerialization </summary>
        /// <returns>Copy</returns>
        [Benchmark(Description = nameof(Below), Baseline = true)]
        public Assembly[] Below() => _allAssemblies.Below(_belowAssembly);
    }
}