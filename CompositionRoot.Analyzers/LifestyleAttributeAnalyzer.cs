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
        public static DiagnosticDescriptor DiagnosticDescriptor { get; } =
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

        [SuppressMessage("Microsoft.CodeAnalysis.Analyzers", "RS1024", Justification = "Error in SymbolEqualityComparer")]
        private void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            /*
             * 1. Check that declaring type is concrete non abstract type
             */
            var isAbstract = classDeclarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.AbstractKeyword));

            if (isAbstract)
            {
                return;
            }

            /*
             * 2. Check that declaring type is derived from IResolvable
             */
            if (!IsComponent(context, classDeclarationSyntax))
            {
                return;
            }

            /*
             * 3. Find LifestyleAttribute
             */
            if (!IsContainsAttribute(context, classDeclarationSyntax))
            {
                ReportDiagnostic(context, classDeclarationSyntax);
            }
        }

        [SuppressMessage("Microsoft.CodeAnalysis.Analyzers", "RS1024", Justification = "Error in SymbolEqualityComparer")]
        private static bool IsComponent(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclarationSyntax)
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

            ISymbol? GetBaseTypeSymbol(BaseTypeSyntax bts)
            {
                return context.Compilation
                              .GetSemanticModel(bts.SyntaxTree)
                              .GetSymbolInfo(bts.Type)
                              .Symbol;
            }

            var baseSymbols = baseTypes.Select(GetBaseTypeSymbol).ToArray();

            var service = context.Compilation.GetTypeByMetadataName(typeof(IResolvable).FullName);

            bool IsDerivedFromService(INamedTypeSymbol symbol)
            {
                return symbol.Equals(service)
                    || symbol.AllInterfaces.Any(i => i.Equals(service));
            }

            var isComponent = baseSymbols.OfType<INamedTypeSymbol>().Any(IsDerivedFromService);

            return isComponent;
        }

        [SuppressMessage("Microsoft.CodeAnalysis.Analyzers", "RS1024", Justification = "Error in SymbolEqualityComparer")]
        private static bool IsContainsAttribute(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclarationSyntax)
        {
            var attributeList = classDeclarationSyntax.ChildNodes().OfType<AttributeListSyntax>().ToArray();

            if (!attributeList.Any())
            {
                return false;
            }

            ITypeSymbol? GetAttributeSymbol(AttributeSyntax syntax)
            {
                return context.Compilation
                    .GetSemanticModel(syntax.SyntaxTree)
                    .GetTypeInfo(syntax)
                    .Type;
            }

            var attribute = context.Compilation.GetTypeByMetadataName(typeof(LifestyleAttribute).FullName);

            var isContainsAttribute = attributeList
                .SelectMany(z => z.Attributes)
                .Select(GetAttributeSymbol)
                .Any(z => z.Equals(attribute));

            return isContainsAttribute;
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclarationSyntax)
        {
            var diagnostic = Diagnostic.Create(DiagnosticDescriptor, classDeclarationSyntax.Identifier.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
