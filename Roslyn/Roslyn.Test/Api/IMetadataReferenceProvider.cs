namespace SpaceEngineers.Core.Roslyn.Test.Api
{
    using System.Collections.Generic;
    using AutoWiringApi.Abstractions;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// MetadataReference provider for test sources
    /// </summary>
    public interface IMetadataReferenceProvider : ICollectionResolvable<IMetadataReferenceProvider>
    {
        /// <summary>
        /// Receive MetadataReferences
        /// </summary>
        /// <returns>MetadataReference collection</returns>
        IEnumerable<MetadataReference> ReceiveReferences();
    }
}