namespace SpaceEngineers.Core.Roslyn.Test
{
    using System.Collections.Generic;
    using Api;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
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
                       typeof(IDependencyContainer).Assembly.CreateReference(),
                       typeof(LifestyleAttribute).Assembly.CreateReference()
                   };
        }
    }
}