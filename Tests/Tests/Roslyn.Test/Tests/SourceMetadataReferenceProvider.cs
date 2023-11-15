namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System.Collections.Generic;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Extensions;
    using Microsoft.CodeAnalysis;

    /// <inheritdoc />
    [Component(EnLifestyle.Singleton)]
    internal class SourceMetadataReferenceProvider : IMetadataReferenceProvider
    {
        /// <inheritdoc />
        public IEnumerable<MetadataReference> ReceiveReferences()
        {
            yield return typeof(Analysis).Assembly.AsMetadataReference();
        }
    }
}