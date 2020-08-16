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
        private readonly TestReferenceWithoutSystemTypes _original;

        /// <summary> .cctor </summary>
        public DeepCopyBenchmarkSource()
        {
            _original = TestReferenceWithoutSystemTypes.CreateOrInit();
        }

        /// <summary> DeepCopyBySerialization </summary>
        /// <returns>Copy</returns>
        [Benchmark(Description = nameof(DeepCopyBySerialization), Baseline = true)]
        public TestReferenceWithoutSystemTypes DeepCopyBySerialization() => _original.DeepCopyBySerialization();

        /// <summary> DeepCopyByReflection </summary>
        /// <returns>Copy</returns>
        [Benchmark(Description = nameof(DeepCopyByReflection))]
        public TestReferenceWithoutSystemTypes DeepCopyByReflection() => _original.DeepCopy();
    }
}