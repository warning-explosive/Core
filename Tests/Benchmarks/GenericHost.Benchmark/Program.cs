namespace SpaceEngineers.Core.GenericHost.Benchmark
{
    /// <summary>
    /// Program
    /// </summary>
    public static class Program
    {
        /// <summary> Main </summary>
        /// <param name="args">args</param>
        public static void Main(string[] args)
        {
            Benchmarks
                .MessageHandlerMiddlewareBenchmarkTest()
                .Wait();
        }
    }
}