namespace SpaceEngineers.Core.Roslyn.Test.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using Basics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;
    using ValueObjects;

    internal static class AnalysisExtensions
    {
        internal const string CSharpDefaultFileExt = ".cs";

        internal static IEnumerable<SourceFile> AnalysisSources(this DiagnosticAnalyzer analyzer)
        {
            return SolutionExtensions
                  .ProjectFile()
                  .Directory
                  .StepInto("Sources")
                  .StepInto(analyzer.GetType().Name)
                  .GetFiles("*" + AnalysisExtensions.CSharpDefaultFileExt, SearchOption.TopDirectoryOnly)
                  .Select(file =>
                          {
                              using var stream = file.OpenRead();
                              return new SourceFile(file.NameWithoutExtension(), SourceText.From(stream));
                          });
        }

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

        internal static async IAsyncEnumerable<Diagnostic> CompileSolution(this Solution solution,
                                                                           ImmutableArray<DiagnosticAnalyzer> analyzers,
                                                                           ImmutableArray<string> ignoredProjects,
                                                                           ImmutableArray<string> ignoredSources,
                                                                           ImmutableArray<string> ignoredNamespaces)
        {
            var projectTree = solution.GetProjectDependencyGraph();
            foreach (var projectId in projectTree.GetTopologicallySortedProjects())
            {
                var project = solution.GetProject(projectId)
                                      .EnsureNotNull($"Project with {projectId} must exist in solution");

                await foreach (var diagnostic in CompileProject(project, analyzers))
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
                return !ignoredProjects.Contains(project.Name, StringComparer.InvariantCulture);
            }

            bool IsNotIgnoredSource(Diagnostic diagnostic)
            {
                return ignoredSources.All(p => !diagnostic.Location.SourceTree.FilePath.Contains(p, StringComparison.InvariantCulture));
            }

            bool IsNotIgnoredNamespace(Diagnostic diagnostic)
            {
                if (!(diagnostic.Location.SourceTree?.GetRoot() is CompilationUnitSyntax cus))
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

        private static async IAsyncEnumerable<Diagnostic> CompileProject(this Project project, ImmutableArray<DiagnosticAnalyzer> analyzers)
        {
            var options = project.CompilationOptions
                                 .WithPlatform(Platform.AnyCpu)
                                 .WithConcurrentBuild(false)
                                 .WithOptimizationLevel(OptimizationLevel.Debug)
                                 .WithOutputKind(OutputKind.DynamicallyLinkedLibrary)
                                 .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default)
                                 .WithGeneralDiagnosticOption(ReportDiagnostic.Error);

            var compilation = await project.WithCompilationOptions(options)
                                           .GetCompilationAsync()
                                           .ConfigureAwait(false);

            foreach (var diagnostic in compilation.GetDiagnostics())
            {
                yield return diagnostic;
            }

            if (!analyzers.Any())
            {
                yield break;
            }

            var analyzerDiagnostics = await compilation.WithAnalyzers(analyzers)
                                                       .GetAnalyzerDiagnosticsAsync()
                                                       .ConfigureAwait(false);

            foreach (var diagnostic in analyzerDiagnostics)
            {
                yield return diagnostic;
            }
        }

        private static Solution AddDocument(this Solution solution, ProjectId projectId, SourceFile sourceFile)
        {
            var sourceFileName = sourceFile.Name + CSharpDefaultFileExt;
            var documentId = DocumentId.CreateNewId(projectId, sourceFileName);
            return solution.AddDocument(documentId, sourceFileName, sourceFile.Text);
        }
    }
}