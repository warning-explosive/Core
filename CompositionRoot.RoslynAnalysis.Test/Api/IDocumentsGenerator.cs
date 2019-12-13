namespace SpaceEngineers.Core.CompositionRoot.RoslynAnalysis.Test.Api
{
    using Abstractions;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Documents generator for test purposes
    /// </summary>
    public interface IDocumentsGenerator : IResolvable
    {
        /// <summary>
        /// Create a Document from a string through creating a project that contains it.
        /// </summary>
        /// <param name="source">Classes in the form of a string</param>
        /// <returns>A Document created from the source string</returns>
        Document CreateDocument(string source);

        /// <summary>
        /// Given an array of strings as sources and a language, turn them into a project and return the documents and spans of it.
        /// </summary>
        /// <param name="sources">Classes in the form of strings</param>
        /// <returns>A Tuple containing the Documents produced from the sources and their TextSpans if relevant</returns>
        /// <returns>Documents for each source</returns>
        Document[] CreateDocuments(string[] sources);
    }
}