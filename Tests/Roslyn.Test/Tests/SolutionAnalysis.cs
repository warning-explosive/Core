namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Core.Test.Api.ClassFixtures;
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
        /// <param name="fixture">TestFixture</param>
        public SolutionAnalysis(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <inheritdoc />
        protected override ImmutableArray<string> IgnoredNamespaces =>
            ImmutableArray.Create("SpaceEngineers.Core.Roslyn.Test.Sources",
                                  "SpaceEngineers.Core.Roslyn.Test.Sources.ComponentAttributeAnalyzer",
                                  "SpaceEngineers.Core.Roslyn.Test.Sources.ComponentAttributeAnalyzerExpected");

        [SuppressMessage("Analysis", "xUnit1004", Justification = "#189")]
        [Fact]
        internal async Task SolutionAnalysisTest()
        {
            var analyzers = DependencyContainer
                           .ResolveCollection<DiagnosticAnalyzer>()
                           .ToImmutableArray();

            var diagnosticsCount = 0;

            var configuration = new Dictionary<string, string>
                                {
                                    {
                                        "BuildingInsideVisualStudio", "false"
                                    }
                                };
            using (var workspace = MSBuildWorkspace.Create(configuration))
            {
                workspace.WorkspaceFailed += (_, workspaceFailedArgs) =>
                {
                    Output.WriteLine(workspaceFailedArgs.Diagnostic.Message);
                };

                await workspace.OpenSolutionAsync(SolutionExtensions.SolutionFile().FullName)
                               .ConfigureAwait(false);

                await foreach (var analyzedDocument in workspace
                                   .CurrentSolution
                                   .CompileSolution(analyzers, IgnoredProjects, IgnoredSources, IgnoredNamespaces)
                                   .WithCancellation(CancellationToken.None)
                                   .ConfigureAwait(false))
                {
                    foreach (var diagnostic in analyzedDocument.ActualDiagnostics)
                    {
                        Interlocked.Increment(ref diagnosticsCount);
                        Output.WriteLine($"[{diagnosticsCount}] " + diagnostic);
                        Output.WriteLine(diagnostic.Location.SourceTree.FilePath + ":" + (diagnostic.Location.GetLineSpan().Span.Start.Line + 1));
                    }
                }
            }

            Assert.Equal(0, diagnosticsCount);
        }
    }
}