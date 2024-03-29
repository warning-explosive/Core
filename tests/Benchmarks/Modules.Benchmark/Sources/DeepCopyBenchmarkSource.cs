namespace SpaceEngineers.Core.Modules.Benchmark.Sources
{
    using System;
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

        private TestReferenceWithoutSystemTypes Original => _original ?? throw new InvalidOperationException(nameof(_original));

        /// <summary>
        /// GlobalSetup
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            _original = TestReferenceWithoutSystemTypes.CreateOrInit();
        }

        /// <summary> DeepCopyByReflection </summary>
        /// <returns>Copy</returns>
        [Benchmark(Description = nameof(DeepCopyByReflection))]
        public TestReferenceWithoutSystemTypes DeepCopyByReflection() => Original.DeepCopy();
    }
}