namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System;
    using Api;
    using AutoWiringApi.Analyzers;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ImplSuffixAnalyzer test
    /// </summary>
    public class ImplSuffixAnalyzerTest : DiagnosticAnalyzerTestBase<ImplSuffixAnalyzer>
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public ImplSuffixAnalyzerTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        internal void WithSuffixTest()
        {
            var test =
@"
namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using SpaceEngineers.Core.CompositionRoot.Abstractions;

    public interface ITestService : IResolvable
    {
    }

    internal class TestServiceImpl : ITestService
    {
    }
}";

            VerifyAnalyzer(test, Array.Empty<DiagnosticResult>());
        }

        [Fact]
        internal void WithoutSuffixTest()
        {
            var test =
@"
namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using SpaceEngineers.Core.CompositionRoot.Abstractions;

    public interface ITestService : IResolvable
    {
    }

    internal class TestService : ITestService
    {
    }
}";

            VerifyAnalyzer(test, Expected(11, 20));
        }
    }
}