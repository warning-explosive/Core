namespace SpaceEngineers.Core.Roslyn.Test.Internals
{
    using System.Collections.Generic;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Extensions;
    using Microsoft.CodeAnalysis;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class CompositionRootMetadataReferenceProvider : IMetadataReferenceProvider
    {
        /// <inheritdoc />
        public IEnumerable<MetadataReference> ReceiveReferences()
        {
            return new[]
                   {
                       typeof(IDependencyContainer).Assembly.AsMetadataReference(),
                       typeof(LifestyleAttribute).Assembly.AsMetadataReference()
                   };
        }
    }
}