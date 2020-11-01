namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System.Collections.Generic;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Extensions;
    using Microsoft.CodeAnalysis;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class SourceMetadataReferenceProvider : IMetadataReferenceProvider
    {
        /// <inheritdoc />
        public IEnumerable<MetadataReference> ReceiveReferences()
        {
            yield return typeof(Analysis).Assembly.AsMetadataReference();
        }
    }
}