namespace SpaceEngineers.Core.CompositionRoot.Analyzers
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// DiagnosticAnalyzer that requres LifestyleAttribute existance on components (component - service implementation)
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LifestyleAttributeAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// DiagnosticDescriptor
        /// </summary>
        public DiagnosticDescriptor DiagnosticDescriptor { get; } =
            new DiagnosticDescriptor("CR1",
                                     "Concrete component must have LifestyleAttribute",
                                     "Mark component type by LifestyleAttribute and select its lifestyle",
                                     "DI Configuration",
                                     DiagnosticSeverity.Error,
                                     true,
                                     string.Empty,
                                     "https://github.com/warning-explosive/Core");

        /// <summary>
        /// SupportedDiagnostics
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptor);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}
