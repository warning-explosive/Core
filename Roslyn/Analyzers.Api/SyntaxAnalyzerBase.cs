namespace SpaceEngineers.Core.Analyzers.Api
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// SyntaxAnalyzerBase
    /// </summary>
    public abstract class SyntaxAnalyzerBase : DiagnosticAnalyzer,
                                               IIdentifiedAnalyzer
    {
        private const string Branch = "master";

        /// <summary>
        /// Diagnostic identifier
        /// </summary>
        public abstract string Identifier { get; }

        /// <summary>
        /// Diagnostic title
        /// </summary>
        public abstract string Title { get; }

        /// <summary>
        /// Diagnostic error message
        /// </summary>
        public abstract string Message { get; }

        /// <summary>
        /// Diagnostic category
        /// </summary>
        public abstract string Category { get; }

        /// <summary>
        /// DiagnosticDescriptor
        /// </summary>
        public DiagnosticDescriptor DiagnosticDescriptor =>
            new DiagnosticDescriptor(Identifier,
                                     Title,
                                     Message,
                                     Category,
                                     DiagnosticSeverity.Error,
                                     true,
                                     string.Empty,
                                     $"https://github.com/warning-explosive/Core/blob/{Branch}/{Assembly}/{DocumentationFileName}");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptor);

        /// <summary>
        /// SyntaxKind of analyzer
        /// </summary>
        protected abstract SyntaxKind SyntaxKind { get; }

        /// <summary>
        /// Documentation file name
        /// </summary>
        private string Assembly => GetType().Assembly.GetName().Name;

        /// <summary>
        /// Documentation file name
        /// </summary>
        private string DocumentationFileName => GetType().Name + ".md";

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind);
        }

        /// <summary>
        /// Analyze syntax node
        /// </summary>
        /// <param name="context">SyntaxNodeAnalysisContext</param>
        protected abstract void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context);

        /// <summary>
        /// Report about diagnostic
        /// </summary>
        /// <param name="context">SyntaxNodeAnalysisContext</param>
        /// <param name="location">Diagnostic location</param>
        /// <param name="args">Message args</param>
        protected void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            Location location,
            params object[] args)
        {
            var diagnostic = Diagnostic.Create(DiagnosticDescriptor, location, args);
            context.ReportDiagnostic(diagnostic);
        }
    }
}