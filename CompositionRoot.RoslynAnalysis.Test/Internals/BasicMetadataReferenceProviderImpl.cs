namespace SpaceEngineers.Core.CompositionRoot.RoslynAnalysis.Test.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using Api;
    using Attributes;
    using Enumerations;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Transient)]
    internal class BasicMetadataReferenceProviderImpl : IMetadataReferenceProvider
    {
        private readonly MetadataReference _corlibReference = typeof(object).Assembly.CreateReference();

        private readonly MetadataReference _systemCoreReference = typeof(Enumerable).Assembly.CreateReference();

        private readonly MetadataReference _cSharpSymbolsReference = typeof(CSharpCompilation).Assembly.CreateReference();

        private readonly MetadataReference _codeAnalysisReference = typeof(Compilation).Assembly.CreateReference();

        /// <inheritdoc />
        public IEnumerable<MetadataReference> ReceiveReferences()
        {
            return new[]
                   {
                       _corlibReference,
                       _systemCoreReference,
                       _cSharpSymbolsReference,
                       _codeAnalysisReference
                   };
        }
    }
}