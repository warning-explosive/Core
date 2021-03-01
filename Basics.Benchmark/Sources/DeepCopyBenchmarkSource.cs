namespace Basics.Benchmark.Sources
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Engines;
    using SpaceEngineers.Core.Basics;
    using SpaceEngineers.Core.Basics.Test.DeepCopy;

    /// <summary>
    /// ObjectExtensions.DeepCopy benchmark source
    /// </summary>
    [SimpleJob(RunStrategy.Throughput)]
    public class DeepCopyBenchmarkSource
    {
        private TestReferenceWithoutSystemTypes? _original;

        private TestReferenceWithoutSystemTypes Original => _original.EnsureNotNull(nameof(_original));

        /// <summary>
        /// GlobalSetup
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            _original = TestReferenceWithoutSystemTypes.CreateOrInit();
        }

        /// <summary> DeepCopyBySerialization </summary>
        /// <returns>Copy</returns>
        [Benchmark(Description = nameof(DeepCopyBySerialization), Baseline = true)]
        public TestReferenceWithoutSystemTypes DeepCopyBySerialization() => Original.DeepCopyBySerialization();

        /// <summary> DeepCopyByReflection </summary>
        /// <returns>Copy</returns>
        [Benchmark(Description = nameof(DeepCopyByReflection))]
        public TestReferenceWithoutSystemTypes DeepCopyByReflection() => Original.DeepCopy();
    }
}