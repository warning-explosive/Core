namespace SpaceEngineers.Core.CompositionRoot.Analyzers
{
    using System;
    using Basics.Roslyn;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Concrete component must have 'Impl' suffix in class name (component - service implementation)
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ImplSuffixAnalyzer : SyntaxAnalyzerBase
    {
        /// <inheritdoc />
        public override string Identifier { get; } = "CR2";

        /// <inheritdoc />
        public override string Title { get; } = "Concrete component must have 'Impl' suffix in class name";

        /// <inheritdoc />
        public override string Message { get; } = "Add 'Impl' suffix in component class name";

        /// <inheritdoc />
        public override string Category { get; } = "DI Configuration";

        /// <inheritdoc />
        protected override SyntaxKind SyntaxKind { get; } = SyntaxKind.ClassDeclaration;

        /// <inheritdoc />
        protected override void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            if (!classDeclarationSyntax.Identifier.Text.EndsWith("Impl", StringComparison.InvariantCulture))
            {
                ReportDiagnostic(context, classDeclarationSyntax.Identifier.GetLocation());
            }
        }
    }
}