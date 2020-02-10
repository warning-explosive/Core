namespace SpaceEngineers.Core.Roslyn.Test
{
    using System;
    using Api;
    using AutoWiringApi.Analyzers;
    using Tests;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// LifestyleAttributeAnalyser and LifestyleAttributeCodeFixProvider tests
    /// </summary>
    public class LifestyleAttributeAnalyserTest : CodeFixProviderTestBase<LifestyleAttributeAnalyzer, LifestyleAttributeCodeFixProvider>
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public LifestyleAttributeAnalyserTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        internal void EmptyAttributesListTest()
        {
            var test =
@"
namespace SpaceEngineers.Core.Roslyn.Test
{
    using System;
    using SpaceEngineers.Core.AutoWiringApi.Abstractions;

    public interface ITestService : IResolvable
    {
    }

    internal class TestServiceImpl : ITestService
    {
    }
}";

            VerifyAnalyzer(test, Expected(11, 20));
        }

        [Fact]
        internal void NotEmptyAttributesListTest()
        {
            var test =
@"
namespace SpaceEngineers.Core.Roslyn.Test
{
    using System;
    using SpaceEngineers.Core.AutoWiringApi.Abstractions;

    public interface ITestService : IResolvable, ICollectionResolvable
    {
    }

    [Serializable]
    internal class TestServiceImpl : ITestService
    {
    }
}";

            VerifyAnalyzer(test, Expected(12, 20));
        }

        [Fact]
        internal void ExistedAttributeTest()
        {
            var test =
@"
namespace SpaceEngineers.Core.Roslyn.Test
{
    using System;
    using SpaceEngineers.Core.AutoWiringApi.Abstractions;
    using SpaceEngineers.Core.AutoWiringApi.Attributes;

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
namespace SpaceEngineers.Core.Roslyn.Test
{
    using System;
    using SpaceEngineers.Core.AutoWiringApi.Abstractions;

    public interface ITestService : IResolvable
    {
    }

    internal class TestServiceImpl : ITestService
    {
    }
}";

            VerifyAnalyzer(test, Expected(11, 20));

            var expectedSource =
@"
namespace SpaceEngineers.Core.Roslyn.Test
{
    using System;
    using SpaceEngineers.Core.AutoWiringApi.Abstractions;
    using SpaceEngineers.Core.AutoWiringApi.Attributes;
    using SpaceEngineers.Core.AutoWiringApi.Enumerations;

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
namespace SpaceEngineers.Core.Roslyn.Test
{
    using System;
    using SpaceEngineers.Core.AutoWiringApi.Abstractions;

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

            VerifyAnalyzer(test, Expected(17, 20));

            var expectedSource =
@"
namespace SpaceEngineers.Core.Roslyn.Test
{
    using System;
    using SpaceEngineers.Core.AutoWiringApi.Abstractions;
    using SpaceEngineers.Core.AutoWiringApi.Attributes;
    using SpaceEngineers.Core.AutoWiringApi.Enumerations;

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
namespace SpaceEngineers.Core.Roslyn.Test
{
    using System;
    using SpaceEngineers.Core.AutoWiringApi.Abstractions;

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

            VerifyAnalyzer(test, Expected(18, 20));

            var expectedSource =
@"
namespace SpaceEngineers.Core.Roslyn.Test
{
    using System;
    using SpaceEngineers.Core.AutoWiringApi.Abstractions;
    using SpaceEngineers.Core.AutoWiringApi.Attributes;
    using SpaceEngineers.Core.AutoWiringApi.Enumerations;

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
