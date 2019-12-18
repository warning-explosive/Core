namespace SpaceEngineers.Core.CompositionRoot.Roslyn.Test.Api
{
    using System.Collections.Generic;
    using Abstractions;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// MetadataReference provider for test sources
    /// </summary>
    public interface IMetadataReferenceProvider : ICollectionResolvable
    {
        /// <summary>
        /// Receive MetadataReferences
        /// </summary>
        /// <returns>MetadataReference collection</returns>
        IEnumerable<MetadataReference> ReceiveReferences();
    }
}