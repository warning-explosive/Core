namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Roslyn;
    using Extensions;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.MSBuild;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Solution analysis test
    /// </summary>
    [Collection(nameof(SolutionAnalysis))]
    public class SolutionAnalysis : AnalysisBase
    {
        private static readonly ImmutableArray<string> IgnoredProjects
            = ImmutableArray<string>.Empty;

        private static readonly ImmutableArray<string> IgnoredSources
            = ImmutableArray.Create("AssemblyAttributes",
                                    "Microsoft.NET.Test.Sdk.Program");

        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public SolutionAnalysis(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <inheritdoc />
        protected override ImmutableArray<string> IgnoredNamespaces =>
            ImmutableArray.Create("SpaceEngineers.Core.Roslyn.Test.Sources",
                                  "SpaceEngineers.Core.Roslyn.Test.Sources.LifestyleAttributeAnalyzer",
                                  "SpaceEngineers.Core.Roslyn.Test.Sources.LifestyleAttributeAnalyzerExpected");

        [SuppressMessage("xUnit.Analyzers", "xUnit1004", Justification = "appveyor environment")]
        [Fact(Skip = "aka.ms/RegisterMSBuildLocator")]
        internal async Task SolutionAnalysisTest()
        {
            var analyzers = DependencyContainer
                           .ResolveCollection<IIdentifiedAnalyzer>()
                           .OfType<DiagnosticAnalyzer>()
                           .ToImmutableArray();

            var totalErrorsCount = 0;

            var configuration = new Dictionary<string, string>
                                {
                                    {
                                        "BuildingInsideVisualStudio", "false"
                                    }
                                };
            using (var workspace = MSBuildWorkspace.Create(configuration))
            {
                workspace.WorkspaceFailed += (sender, workspaceFailedArgs) =>
                                             {
                                                 Output.WriteLine(workspaceFailedArgs.Diagnostic.Message);
                                             };
                await workspace.OpenSolutionAsync(SolutionExtensions.SolutionFile().FullName)
                               .ConfigureAwait(false);

                await foreach (var analyzedDocument in workspace.CurrentSolution.CompileSolution(analyzers, IgnoredProjects, IgnoredSources, IgnoredNamespaces))
                {
                    foreach (var diagnostic in analyzedDocument.ActualDiagnostics)
                    {
                        totalErrorsCount++;
                        Output.WriteLine(diagnostic.ToString());
                    }
                }
            }

            Assert.Equal(0, totalErrorsCount);
        }
    }
}