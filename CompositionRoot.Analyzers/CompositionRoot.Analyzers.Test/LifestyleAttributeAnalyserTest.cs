namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System.Globalization;
    using Analyzers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using RoslynAnalysis.Test;
    using RoslynAnalysis.Test.Api;
    using Xunit;
    using Xunit.Abstractions;

    public class LifestyleAttributeAnalyserTest : RoslynAnalysisTestBase
    {
        public LifestyleAttributeAnalyserTest(ITestOutputHelper output)
            : base(output)
        {
        }

        protected override DiagnosticAnalyzer DiagnosticAnalyzer { get; } = new LifestyleAttributeAnalyzer();

        protected override CodeFixProvider CodeFixProvider { get; } = new LifestyleAttributeCodeFixProvider();

        [Fact]
        internal void DiagnosticTest()
        {
            var test =
@"
using Attributes;
using Enumerations;

[Lifestyle(EnLifestyle.Singleton)]
internal class SingletonTestServiceImpl : ISingletonTestService
{
}";

            var expected = new DiagnosticResult("CompositionRootAnalyzers",
                                                string.Format(CultureInfo.InvariantCulture, "Type name '{0}' contains lowercase letters", "TypeName"),
                                                DiagnosticSeverity.Warning,
                                                new[] { new DiagnosticResultLocation("Source0.cs", 11, 15) });

            VerifyAnalyzer(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {
        }
    }";
            VerifyFix(test, fixtest);
        }
    }
}
