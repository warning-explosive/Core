namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Abstractions;
    using Analyzers.Api;
    using Basics.Exceptions;
    using Core.Test.Api.ClassFixtures;
    using Extensions;
    using Internals;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
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
        /// <param name="fixture">ModulesTestFixture</param>
        public Analysis(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <inheritdoc />
        protected override ImmutableArray<string> IgnoredNamespaces =>
            ImmutableArray.Create("SpaceEngineers.Core.Roslyn.Test.Sources",
                                  "SpaceEngineers.Core.Roslyn.Test.Sources.ComponentAttributeAnalyzer",
                                  "SpaceEngineers.Core.Roslyn.Test.Sources.ComponentAttributeAnalyzerExpected");

        [Fact]
        internal async Task AnalysisTest()
        {
            var analyzers = DependencyContainer
                .ResolveCollection<IIdentifiedAnalyzer>()
                .OfType<SyntaxAnalyzerBase>();

            foreach (var analyzer in analyzers)
            {
                var conventionalProvider = DependencyContainer.Resolve<IConventionalProvider>();
                var codeFixProvider = conventionalProvider.CodeFixProvider(analyzer);

                await TestSingleAnalyzer(analyzer, codeFixProvider, conventionalProvider).ConfigureAwait(false);
            }
        }

        [SuppressMessage("Analysis", "CA1506", Justification = "Reviewed")]
        private async Task TestSingleAnalyzer(SyntaxAnalyzerBase analyzer,
                                              CodeFixProvider? codeFix,
                                              IConventionalProvider conventionalProvider)
        {
            var analyzerVerifier = DependencyContainer.Resolve<IDiagnosticAnalyzerVerifier>();
            var codeFixVerifier = DependencyContainer.Resolve<ICodeFixVerifier>();
            var metadataReferences = DependencyContainer
                                    .ResolveCollection<IMetadataReferenceProvider>()
                                    .SelectMany(p => p.ReceiveReferences())
                                    .Distinct()
                                    .ToArray();

            using (var workspace = new AdhocWorkspace())
            {
                workspace.WorkspaceFailed += (sender, workspaceFailedArgs) =>
                                             {
                                                 Output.WriteLine(workspaceFailedArgs.Diagnostic.Message);
                                             };

                var analyzerSources = conventionalProvider.SourceFiles(analyzer);
                var diagnostics = workspace
                                 .CurrentSolution
                                 .SetupSolution(GetType().Name, analyzerSources, metadataReferences)
                                 .CompileSolution(ImmutableArray.Create((DiagnosticAnalyzer)analyzer),
                                                  IgnoredProjects,
                                                  IgnoredSources,
                                                  ImmutableArray<string>.Empty);

                var expectedDiagnostics = conventionalProvider
                                         .ExpectedDiagnosticsProvider(analyzer)
                                         .ByFileName(analyzer);
                var expectedFixedSources = conventionalProvider
                                          .SourceFiles(analyzer, "Expected")
                                          .ToDictionary(s => s.Name);

                await foreach (var analyzedDocument in diagnostics)
                {
                    foreach (var diagnostic in analyzedDocument.ActualDiagnostics)
                    {
                        Output.WriteLine(diagnostic.ToString());
                    }

                    if (!expectedDiagnostics.Remove(analyzedDocument.Name, out var expected))
                    {
                        throw new InvalidOperationException($"Unsupported source file: {analyzedDocument.Name}");
                    }

                    analyzerVerifier.VerifyAnalyzedDocument(analyzer, analyzedDocument, expected);

                    if (codeFix != null
                     && expectedFixedSources.Remove(analyzedDocument.Name + Conventions.Expected, out var expectedSource))
                    {
                        await codeFixVerifier.VerifyCodeFix(analyzer, codeFix, analyzedDocument, expectedSource, Output.WriteLine).ConfigureAwait(false);
                    }
                }

                if (expectedDiagnostics.Any())
                {
                    var files = string.Join(", ", expectedDiagnostics.Keys);
                    throw new InvalidOperationException($"Ambiguous diagnostics in files: {files}");
                }

                if (expectedFixedSources.Any())
                {
                    if (codeFix == null)
                    {
                        throw new NotFoundException($"Specify code fix for: {analyzer.GetType().Name}");
                    }

                    throw new InvalidOperationException($"Ambiguous expected codeFix sources: {string.Join(", ", expectedFixedSources.Keys)}");
                }
            }
        }
    }
}