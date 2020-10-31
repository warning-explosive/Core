namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Abstractions;
    using Basics.Roslyn;
    using Extensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// AnalyzerTest
    /// </summary>
    [Collection(nameof(Analysis))]
    public class Analysis : AnalysisBase
    {
        private static readonly ImmutableArray<string> IgnoredProjects
            = ImmutableArray<string>.Empty;

        private static readonly ImmutableArray<string> IgnoredSources
            = ImmutableArray.Create(nameof(IExpectedDiagnosticsProvider).Substring(1, nameof(IExpectedDiagnosticsProvider).Length - 1));

        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public Analysis(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <inheritdoc />
        protected override ImmutableArray<string> IgnoredNamespaces =>
            ImmutableArray.Create("SpaceEngineers.Core.Roslyn.Test.Sources.LifestyleAttributeAnalyzer");

        [Fact]
        internal async Task AnalysisTest()
        {
            var testCases = DependencyContainer
                           .ResolveCollection<IIdentifiedAnalyzer>()
                           .OfType<SyntaxAnalyzerBase>()
                           .Select(analyzer => (analyzer, analyzer.AnalysisSources().ToArray()));

            foreach (var (analyzer, sources) in testCases)
            {
                await TestSingleAnalyzer(analyzer, sources).ConfigureAwait(false);
            }
        }

        private async Task TestSingleAnalyzer(SyntaxAnalyzerBase analyzer, SourceFile[] sources)
        {
            IDiagnosticAnalyzerVerifier verifier = DependencyContainer.Resolve<IDiagnosticAnalyzerVerifier>();
            var metadataReferences = DependencyContainer
                                    .ResolveCollection<IMetadataReferenceProvider>()
                                    .SelectMany(p => p.ReceiveReferences())
                                    .ToArray();

            using (var workspace = new AdhocWorkspace())
            {
                workspace.WorkspaceFailed += (sender, workspaceFailedArgs) =>
                                             {
                                                 Output.WriteLine(workspaceFailedArgs.Diagnostic.Message);
                                             };

                var diagnostics = workspace
                                 .CurrentSolution
                                 .SetupSolution(GetType().Name, sources, metadataReferences)
                                 .CompileSolution(ImmutableArray.Create((DiagnosticAnalyzer)analyzer), IgnoredProjects, IgnoredSources, ImmutableArray<string>.Empty)
                                 .ConfigureAwait(false);

                var actualDiagnostics = new List<Diagnostic>();

                await foreach (var diagnostic in diagnostics)
                {
                    actualDiagnostics.Add(diagnostic);
                    Output.WriteLine(diagnostic.ToString());
                }

                verifier.VerifyDiagnosticsGroup(analyzer, actualDiagnostics.ToArray());
            }

            // TODO: Execute fix
            // TODO: verify fix
        }
    }
}