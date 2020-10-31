namespace SpaceEngineers.Core.Roslyn.Test.Internals
{
    using System.Collections.Generic;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Extensions;
    using Microsoft.CodeAnalysis;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class DotNetSdkMetadataReferenceProvider : IMetadataReferenceProvider
    {
        /// <inheritdoc />
        public IEnumerable<MetadataReference> ReceiveReferences()
        {
            var frameworkDirectory = typeof(object).Assembly.Location.AsFileInfo().Directory;

            yield return frameworkDirectory.GetFile("netstandard", ".dll").AsMetadataReference();
            yield return frameworkDirectory.GetFile("mscorlib", ".dll").AsMetadataReference();
            yield return frameworkDirectory.GetFile("System.Private.CoreLib", ".dll").AsMetadataReference();
            yield return frameworkDirectory.GetFile("System.Runtime", ".dll").AsMetadataReference();
            yield return frameworkDirectory.GetFile("System.Reflection", ".dll").AsMetadataReference();
        }
    }
}