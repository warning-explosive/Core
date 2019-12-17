namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using System.Globalization;
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
        internal void FixWithoutLeadingTriviaTest()
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

            var expectedSource =
@"
namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using SpaceEngineers.Core.CompositionRoot.Abstractions;
    using SpaceEngineers.Core.CompositionRoot.Attributes;
    using SpaceEngineers.Core.CompositionRoot.Enumerations;

    public interface ITestService : IResolvable
    {
    }

    [Lifestyle(EnLifestyle.ChooseLifestyle)]
    internal class TestServiceImpl : ITestService
    {
    }
}";

            VerifyFix(test, expectedSource, true);
        }

        [Fact]
        internal void FixWithLeadingTriviaTest()
        {
            var test =
@"
namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using SpaceEngineers.Core.CompositionRoot.Abstractions;

    /// <summary>
    /// Summary
    /// </summary>
    public interface ITestService : IResolvable
    {
    }

    /// <summary>
    /// Summary
    /// </summary>
    internal class TestServiceImpl : ITestService
    {
    }
}";

            var expected = new DiagnosticResult("CR1",
                                                string.Format(CultureInfo.InvariantCulture, "Mark component type by LifestyleAttribute and select its lifestyle"),
                                                DiagnosticSeverity.Error,
                                                new[] { new DiagnosticResultLocation("Source0.cs", 17, 20) });

            VerifyAnalyzer(test, expected);

            var expectedSource =
@"
namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using SpaceEngineers.Core.CompositionRoot.Abstractions;
    using SpaceEngineers.Core.CompositionRoot.Attributes;
    using SpaceEngineers.Core.CompositionRoot.Enumerations;

    /// <summary>
    /// Summary
    /// </summary>
    public interface ITestService : IResolvable
    {
    }

    /// <summary>
    /// Summary
    /// </summary>
    [Lifestyle(EnLifestyle.ChooseLifestyle)]
    internal class TestServiceImpl : ITestService
    {
    }
}";

            VerifyFix(test, expectedSource, true);
        }

        [Fact]
        internal void FixWithLeadingTriviaAndAttributesTest()
        {
            var test =
@"
namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using SpaceEngineers.Core.CompositionRoot.Abstractions;

    /// <summary>
    /// Summary
    /// </summary>
    public interface ITestService : IResolvable
    {
    }

    /// <summary>
    /// Summary
    /// </summary>
    [Serializable]
    internal class TestServiceImpl : ITestService
    {
    }
}";

            var expected = new DiagnosticResult("CR1",
                                                string.Format(CultureInfo.InvariantCulture, "Mark component type by LifestyleAttribute and select its lifestyle"),
                                                DiagnosticSeverity.Error,
                                                new[] { new DiagnosticResultLocation("Source0.cs", 18, 20) });

            VerifyAnalyzer(test, expected);

            var expectedSource =
@"
namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System;
    using SpaceEngineers.Core.CompositionRoot.Abstractions;
    using SpaceEngineers.Core.CompositionRoot.Attributes;
    using SpaceEngineers.Core.CompositionRoot.Enumerations;

    /// <summary>
    /// Summary
    /// </summary>
    public interface ITestService : IResolvable
    {
    }

    /// <summary>
    /// Summary
    /// </summary>
    [Serializable]
    [Lifestyle(EnLifestyle.ChooseLifestyle)]
    internal class TestServiceImpl : ITestService
    {
    }
}";

            VerifyFix(test, expectedSource, true);
        }
    }
}
