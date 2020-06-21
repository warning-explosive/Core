namespace SpaceEngineers.Core.Roslyn.Test.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class DocumentsGeneratorImpl : IDocumentsGenerator
    {
        private const string DefaultFilePathPrefix = "Source";
        private const string CSharpDefaultFileExt = "cs";
        private const string TestProjectName = "AnalysisProject";

        private readonly IEnumerable<IMetadataReferenceProvider> _providers;

        public DocumentsGeneratorImpl(IEnumerable<IMetadataReferenceProvider> providers)
        {
            _providers = providers;
        }

        /// <summary>
        /// Create a Document from a string through creating a project that contains it.
        /// </summary>
        /// <param name="source">Classes in the form of a string</param>
        /// <returns>A Document created from the source string</returns>
        public Document CreateDocument(string source)
        {
            return CreateProject(new[] { source }).Documents.First();
        }

        /// <summary>
        /// Given an array of strings as sources and a language, turn them into a project and return the documents and spans of it.
        /// </summary>
        /// <param name="sources">Classes in the form of strings</param>
        /// <returns>A Tuple containing the Documents produced from the sources and their TextSpans if relevant</returns>
        /// <returns>Documents for each source</returns>
        public Document[] CreateDocuments(string[] sources)
        {
            var project = CreateProject(sources);
            var documents = project.Documents.ToArray();

            if (sources.Length != documents.Length)
            {
                throw new InvalidOperationException("Amount of sources did not match amount of Documents created");
            }

            return documents;
        }

        private Project CreateProject(string[] sources)
        {
            var projectId = ProjectId.CreateNewId(TestProjectName);

            var metadataReferences = _providers.SelectMany(z => z.ReceiveReferences())
                                               .Distinct()
                                               .ToArray();

            using (var workspace = new AdhocWorkspace())
            {
                var solution = workspace.CurrentSolution
                                        .AddProject(projectId, TestProjectName, TestProjectName, LanguageNames.CSharp)
                                        .AddMetadataReferences(projectId, metadataReferences);

                sources.Each((source, i) =>
                             {
                                 var newFileName = DefaultFilePathPrefix + i + "." + CSharpDefaultFileExt;
                                 var documentId = DocumentId.CreateNewId(projectId, newFileName);
                                 solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
                             });

                return solution.GetProject(projectId).TryExtractFromNullable($"{nameof(workspace.CurrentSolution)} must contains project {projectId}");
            }
        }
    }
}