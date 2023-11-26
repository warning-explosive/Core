namespace SpaceEngineers.Core.Roslyn.Test.MetadataReferenceProviders
{
    using System;
    using System.Collections.Generic;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Extensions;
    using Microsoft.CodeAnalysis;

    /// <inheritdoc />
    [Component(EnLifestyle.Singleton)]
    internal class DotNetSdkMetadataReferenceProvider : IMetadataReferenceProvider
    {
        /// <inheritdoc />
        public IEnumerable<MetadataReference> ReceiveReferences()
        {
            var frameworkDirectory = typeof(object)
                                         .Assembly
                                         .Location
                                         .AsFileInfo()
                                         .Directory
                                     ?? throw new InvalidOperationException(".NET Framework directory wasn't found");

            yield return frameworkDirectory.GetFile("netstandard", ".dll").AsMetadataReference();
            yield return frameworkDirectory.GetFile("mscorlib", ".dll").AsMetadataReference();
            yield return frameworkDirectory.GetFile("System.Private.CoreLib", ".dll").AsMetadataReference();
            yield return frameworkDirectory.GetFile("System.Runtime", ".dll").AsMetadataReference();
            yield return frameworkDirectory.GetFile("System.Reflection", ".dll").AsMetadataReference();
        }
    }
}