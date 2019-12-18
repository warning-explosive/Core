namespace SpaceEngineers.Core.CompositionRoot.Analyzers.Test
{
    using System.Collections.Generic;
    using Attributes;
    using Enumerations;
    using Microsoft.CodeAnalysis;
    using Roslyn.Test.Api;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Transient)]
    internal class CompositionRootMetadataReferenceProvider : IMetadataReferenceProvider
    {
        /// <inheritdoc />
        public IEnumerable<MetadataReference> ReceiveReferences()
        {
            return new[] { typeof(DependencyContainer).Assembly.CreateReference() };
        }
    }
}