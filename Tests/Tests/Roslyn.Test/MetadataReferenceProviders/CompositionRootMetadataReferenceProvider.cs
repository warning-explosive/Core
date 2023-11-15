namespace SpaceEngineers.Core.Roslyn.Test.MetadataReferenceProviders
{
    using System.Collections.Generic;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Extensions;
    using Microsoft.CodeAnalysis;

    /// <inheritdoc />
    [Component(EnLifestyle.Singleton)]
    internal class CompositionRootMetadataReferenceProvider : IMetadataReferenceProvider
    {
        /// <inheritdoc />
        public IEnumerable<MetadataReference> ReceiveReferences()
        {
            return new[]
                   {
                       AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Basics))).AsMetadataReference(),
                       AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(AutoRegistration), nameof(AutoRegistration.Api))).AsMetadataReference(),
                       AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot))).AsMetadataReference()
                   };
        }
    }
}