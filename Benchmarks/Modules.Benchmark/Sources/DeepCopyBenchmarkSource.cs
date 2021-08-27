namespace SpaceEngineers.Core.Modules.Benchmark.Sources
{
    using Basics;
    using Basics.Test.DeepCopy;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Engines;

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