namespace SpaceEngineers.Core.Modules.Benchmark.Sources
{
    using System;
    using System.IO;
    using Basics;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Engines;

    /// <summary>
    /// Measures Stream.CopyTo vs. Stream.Read
    /// </summary>
    [SimpleJob(RunStrategy.Throughput)]
    public class StreamCopyVersusStreamReadBenchmarkSource
    {
        private FileInfo? _file;

        /// <summary>
        /// GlobalSetup
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            _file = SolutionExtensions
                .SolutionFile()
                .Directory
                .EnsureNotNull("Unable to find solution directory")
                .StepInto("Benchmarks")
                .StepInto("Modules.Benchmark")
                .StepInto("Settings")
                .GetFile("appsettings", ".json");
        }

        /// <summary> CopyTo </summary>
        /// <returns>Object</returns>
        [Benchmark(Description = nameof(CopyTo), Baseline = true)]
        public ReadOnlyMemory<byte> CopyTo()
        {
            using (var stream = File.Open(_file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);

                return memoryStream.AsBytes();
            }
        }

        /// <summary> CopyTo </summary>
        /// <returns>Object</returns>
        [Benchmark(Description = nameof(Read))]
        public ReadOnlyMemory<byte> Read()
        {
            using (var stream = File.Open(_file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var length = (int)stream.Length;
                var buffer = new byte[length];

                stream.Position = 0;

                _ = stream.Read(buffer);

                return buffer.AsMemory();
            }
        }
    }
}