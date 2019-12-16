namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using System.Globalization;
    using Analyzers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using RoslynAnalysis.Test;
    using RoslynAnalysis.Test.Api;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// LifestyleAttributeAnalyser and LifestyleAttributeCodeFixProvider tests
    /// </summary>
    public class LifestyleAttributeAnalyserTest : RoslynAnalysisTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public LifestyleAttributeAnalyserTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <inheritdoc />
        protected override DiagnosticAnalyzer DiagnosticAnalyzer { get; } = new LifestyleAttributeAnalyzer();

        /// <inheritdoc />
        protected override CodeFixProvider CodeFixProvider { get; } = new LifestyleAttributeCodeFixProvider();

        [Fact]
        internal void EmptyAttributesListTest()
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

            var expected = new DiagnosticResult("CR1",
                                                string.Format(CultureInfo.InvariantCulture, "Mark component type by LifestyleAttribute and select its lifestyle"),
                                                DiagnosticSeverity.Error,
                                                new[] { new DiagnosticResultLocation("Source0.cs", 11, 20) });

            VerifyAnalyzer(test, expected);
        }

        [Fact]
        internal void NotEmptyAttributesListTest()
        {
            var test =
@"
namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using SpaceEngineers.Core.CompositionRoot.Abstractions;

    public interface ITestService : IResolvable, ICollectionResolvable
    {
    }

    [Serializable]
    internal class TestServiceImpl : ITestService
    {
    }
}";

            var expected = new DiagnosticResult("CR1",
                                                string.Format(CultureInfo.InvariantCulture, "Mark component type by LifestyleAttribute and select its lifestyle"),
                                                DiagnosticSeverity.Error,
                                                new[] { new DiagnosticResultLocation("Source0.cs", 12, 20) });

            VerifyAnalyzer(test, expected);
        }

        [Fact]
        internal void ExistedAttributeTest()
        {
            var test =
@"
namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using SpaceEngineers.Core.CompositionRoot.Abstractions;
    using SpaceEngineers.Core.CompositionRoot.Attributes;

    public interface ITestService : IResolvable
    {
    }

    [Lifestyle(EnLifestyle.Singleton)]
    [Serializable]
    internal class TestServiceImpl : ITestService
    {
    }
}";

            VerifyAnalyzer(test, Array.Empty<DiagnosticResult>());
        }

        [Fact]
        internal void FixTest()
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

    [Serializable]
    internal class TestServiceImpl : ITestService
    {
    }
}";

            var expected = new DiagnosticResult("CR1",
                                                string.Format(CultureInfo.InvariantCulture, "Mark component type by LifestyleAttribute and select its lifestyle"),
                                                DiagnosticSeverity.Error,
                                                new[] { new DiagnosticResultLocation("Source0.cs", 12, 20) });

            VerifyAnalyzer(test, expected);

            var fixtest =
@"
namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using SpaceEngineers.Core.CompositionRoot.Abstractions;
    using SpaceEngineers.Core.CompositionRoot.Attributes;

    public interface ITestService : IResolvable
    {
    }

    [Lifestyle(EnLifestyle.ChooseLifestyle)]
    [Serializable]
    internal class TestServiceImpl : ITestService
    {
    }
}";

            VerifyFix(test, fixtest, true);
        }
    }
}
