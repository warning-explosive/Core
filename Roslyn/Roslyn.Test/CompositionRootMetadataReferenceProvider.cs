namespace SpaceEngineers.Core.Roslyn.Test
{
    using System.Collections.Generic;
    using Api;
    using AutoRegistration;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Microsoft.CodeAnalysis;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Transient)]
    internal class CompositionRootMetadataReferenceProvider : IMetadataReferenceProvider
    {
        /// <inheritdoc />
        public IEnumerable<MetadataReference> ReceiveReferences()
        {
            return new[]
                   {
                       typeof(DependencyContainer).Assembly.CreateReference(),
                       typeof(LifestyleAttribute).Assembly.CreateReference()
                   };
        }
    }
}