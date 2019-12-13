namespace SpaceEngineers.Core.CompositionRoot.Analyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;

    /// <summary>
    /// CompositionRootAnalyzersCodeFixProvider
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LifestyleAttributeCodeFixProvider))]
    public class LifestyleAttributeCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(new LifestyleAttributeAnalyzer().DiagnosticDescriptor.Id);

        /// <inheritdoc />
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc />
        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            throw new NotImplementedException();
        }
    }
}
