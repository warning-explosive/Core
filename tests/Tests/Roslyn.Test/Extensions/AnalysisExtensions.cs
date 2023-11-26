namespace SpaceEngineers.Core.Roslyn.Test.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics.Exceptions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using ValueObjects;

    internal static class AnalysisExtensions
    {
        internal const string CSharpDefaultFileExt = ".cs";

        internal static Solution SetupSolution(this Solution solution,
                                               string projectName,
                                               IEnumerable<SourceFile> sources,
                                               IEnumerable<MetadataReference> metadataReferences)
        {
            var projectId = ProjectId.CreateNewId(projectName);

            var projectInfo = ProjectInfo.Create(projectId,
                                                 VersionStamp.Default,
                                                 projectName,
                                                 projectName,
                                                 LanguageNames.CSharp,
                                                 parseOptions: new CSharpParseOptions(LanguageVersion.Latest),
                                                 metadataReferences: metadataReferences);

            solution = solution.AddProject(projectInfo);

            foreach (var source in sources)
            {
                solution = solution.AddDocument(projectId, source);
            }

            return solution;
        }

        internal static async IAsyncEnumerable<AnalyzedDocument> CompileSolution(
            this Solution solution,
            ImmutableArray<DiagnosticAnalyzer> analyzers,
            ImmutableArray<string> ignoredProjects,
            ImmutableArray<string> ignoredSources,
            ImmutableArray<string> ignoredNamespaces)
        {
            var projectTree = solution.GetProjectDependencyGraph();
            foreach (var projectId in projectTree.GetTopologicallySortedProjects())
            {
                var project = solution.GetProject(projectId)
                              ?? throw new InvalidOperationException($"Project with {projectId} must exist in solution");

                await foreach (var diagnostic in CompileProject(project, analyzers)
                                   .WithCancellation(CancellationToken.None)
                                   .ConfigureAwait(false))
                {
                    if (IsNotIgnoredProject(project)
                        && IsNotIgnoredSource(diagnostic)
                        && IsNotIgnoredNamespace(diagnostic))
                    {
                        yield return diagnostic;
                    }
                }
            }

            bool IsNotIgnoredProject(Project project)
            {
                return !ignoredProjects.Contains(project.Name, StringComparer.OrdinalIgnoreCase);
            }

            bool IsNotIgnoredSource(AnalyzedDocument analyzedDocument)
            {
                return ignoredSources.All(p => !analyzedDocument.Name.Contains(p, StringComparison.OrdinalIgnoreCase));
            }

            bool IsNotIgnoredNamespace(AnalyzedDocument analyzedDocument)
            {
                var syntaxTree = analyzedDocument.Document.GetSyntaxTreeAsync().Result;

                if (syntaxTree?.GetRoot() is not CompilationUnitSyntax cus)
                {
                    return true;
                }

                var namespaceSyntax = cus.Members.OfType<NamespaceDeclarationSyntax>().SingleOrDefault();

                if (namespaceSyntax == null)
                {
                    return true;
                }

                var @namespace = namespaceSyntax.Name.ToString();

                return !ignoredNamespaces.Any(n => n == @namespace);
            }
        }

        private static async IAsyncEnumerable<AnalyzedDocument> CompileProject(
            this Project project,
            ImmutableArray<DiagnosticAnalyzer> analyzers)
        {
            var options = project.CompilationOptions
                                 .WithPlatform(Platform.AnyCpu)
                                 .WithConcurrentBuild(false)
                                 .WithOptimizationLevel(OptimizationLevel.Debug)
                                 .WithOutputKind(OutputKind.DynamicallyLinkedLibrary)
                                 .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default)
                                 .WithGeneralDiagnosticOption(ReportDiagnostic.Error);

            project = project.WithCompilationOptions(options);

            var compilation = await project.GetCompilationAsync().ConfigureAwait(false);

            foreach (var analyzedDocument in GroupByDocument(project, compilation.GetDiagnostics()))
            {
                yield return analyzedDocument;
            }

            if (!analyzers.Any())
            {
                yield break;
            }

            var analyzerDiagnostics = await compilation.WithAnalyzers(analyzers)
                                                       .GetAnalyzerDiagnosticsAsync()
                                                       .ConfigureAwait(false);

            foreach (var analyzedDocument in GroupByDocument(project, analyzerDiagnostics))
            {
                yield return analyzedDocument;
            }
        }

        private static Solution AddDocument(this Solution solution, ProjectId projectId, SourceFile sourceFile)
        {
            var sourceFileName = sourceFile.Name + CSharpDefaultFileExt;
            var documentId = DocumentId.CreateNewId(projectId, sourceFileName);
            return solution.AddDocument(documentId, sourceFileName, sourceFile.Text);
        }

        private static IEnumerable<AnalyzedDocument> GroupByDocument(Project project, IEnumerable<Diagnostic> source)
        {
            return source
                  .Select(diagnostic =>
                          {
                              var document = project.GetDocument(diagnostic.Location.SourceTree);

                              if (document == null)
                              {
                                  throw new NotFoundException("Couldn't find document");
                              }

                              return (document, diagnostic);
                          })
                  .GroupBy(pair => pair.document,
                           pair => pair.diagnostic)
                  .Select(grp => new AnalyzedDocument(grp.Key, ImmutableArray.Create(grp.ToArray())));
        }
    }
}