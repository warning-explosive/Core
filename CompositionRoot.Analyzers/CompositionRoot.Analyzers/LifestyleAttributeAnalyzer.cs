namespace SpaceEngineers.Core.CompositionRoot.Analyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Abstractions;
    using Attributes;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            /*
             * 1. Check that declaring type is derived from IResolvable, and its concrete non abstract type
             */
            var isAbstract = classDeclarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.AbstractKeyword));

            if (isAbstract)
            {
                return;
            }

            if (!IsComponent(context, classDeclarationSyntax))
            {
                return;
            }

            /*
             * 2. find LifestyleAttribute
             */
            var attribute = context.Compilation.GetTypeByMetadataName(typeof(LifestyleAttribute).FullName);

            ISymbol? GetAttributeSymbol(AttributeSyntax syntax)
            {
                return context.Compilation
                              .GetSemanticModel(syntax.SyntaxTree)
                              .GetSymbolInfo(syntax)
                              .Symbol;
            }

            var attributeList = classDeclarationSyntax.ChildNodes()
                                                      .OfType<AttributeListSyntax>();

            if (!attributeList.Any())
            {
                return;
            }

            var attributes = 
                                                   .Single()
                                                   .Attributes
                                                   .Select(GetAttributeSymbol)
                                                   .ToArray();

            var diagnostic = Diagnostic.Create(DiagnosticDescriptor, classDeclarationSyntax.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        [SuppressMessage("Microsoft.CodeAnalysis.Analyzers", "RS1024", Justification = "Error in SymbolEqualityComparer")]
        private static bool IsComponent(SyntaxNodeAnalysisContext context,
                                        ClassDeclarationSyntax classDeclarationSyntax)
        {
            var baseTypes = classDeclarationSyntax.ChildNodes()
                                                  .OfType<BaseListSyntax>()
                                                  .SingleOrDefault()
                                                 ?.Types
                                                  .ToArray() ?? Array.Empty<BaseTypeSyntax>();

            if (!baseTypes.Any())
            {
                return false;
            }

            ISymbol? GetSymbol(BaseTypeSyntax bts)
            {
                return context.Compilation
                              .GetSemanticModel(bts.SyntaxTree)
                              .GetSymbolInfo(bts.Type)
                              .Symbol;
            }

            var baseSymbols = baseTypes.Select(GetSymbol)
                                       .ToArray();

            var service = context.Compilation.GetTypeByMetadataName(typeof(IResolvable).FullName);

            bool IsDerivedFromService(INamedTypeSymbol symbol)
            {
                return symbol.Equals(service)
                    || symbol.AllInterfaces.Any(i => i.Equals(service));
            }

            return baseSymbols.OfType<INamedTypeSymbol>()
                              .Any(IsDerivedFromService);
        }
    }
}
