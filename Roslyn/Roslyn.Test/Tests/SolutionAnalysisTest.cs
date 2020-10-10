namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Basics;
    using Microsoft.Build.Locator;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Solution analysis test
    /// </summary>
    public class SolutionAnalysisTest
    {
        private readonly ITestOutputHelper _output;

        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public SolutionAnalysisTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        internal async Task SolutionTest()
        {
            var totalErrorsCount = 0;

            MSBuildLocator.RegisterDefaults();
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
                                                 _output.WriteLine(workspaceFailedArgs.Diagnostic.Message);
                                             };
                await workspace.OpenSolutionAsync(SolutionExtensions.SolutionFile().FullName)
                               .ConfigureAwait(false);
                var projectTree = workspace.CurrentSolution.GetProjectDependencyGraph();
                foreach (var projectId in projectTree.GetTopologicallySortedProjects())
                {
                    var project = workspace.CurrentSolution
                                           .GetProject(projectId)
                                           .EnsureNotNull($"Project with {projectId} must exist in solution");

                    var errors = await CompileProject(project)
                                    .ConfigureAwait(false);

                    _output.WriteLine($"{project.Name}: {errors.Count}");
                    totalErrorsCount += errors.Count;
                    foreach (var error in errors)
                    {
                        _output.WriteLine($"\t{error}");
                    }
                }
            }

            Assert.Equal(0, totalErrorsCount);
        }

        private static async Task<ICollection<Diagnostic>> CompileProject(Project project)
        {
            var compilation = await project.GetCompilationAsync()
                                           .ConfigureAwait(false);
            return compilation
                  .GetDiagnostics()
                  .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                  .ToList();
        }
    }
}