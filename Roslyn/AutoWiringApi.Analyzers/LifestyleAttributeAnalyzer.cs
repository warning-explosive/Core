namespace SpaceEngineers.Core.AutoWiringApi.Analyzers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Abstractions;
    using Attributes;
    using Basics.Roslyn;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Concrete component must have LifestyleAttribute (component - service implementation)
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LifestyleAttributeAnalyzer : SyntaxAnalyzerBase
    {
        /// <inheritdoc />
        public override string Identifier { get; } = "CR1";

        /// <inheritdoc />
        public override string Title { get; } = $"Concrete component must have {nameof(LifestyleAttribute)}";

        /// <inheritdoc />
        public override string Message { get; } = $"Mark component type by {nameof(LifestyleAttribute)} and select its lifestyle";

        /// <inheritdoc />
        public override string Category { get; } = "DI Configuration";

        /// <inheritdoc />
        protected override SyntaxKind SyntaxKind { get; } = SyntaxKind.ClassDeclaration;

        /// <inheritdoc />
        [SuppressMessage("Microsoft.CodeAnalysis.Analyzers", "RS1024", Justification = "Error in SymbolEqualityComparer")]
        protected override void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
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
                ReportDiagnostic(context, classDeclarationSyntax.Identifier.GetLocation());
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

            var resolvable = context.Compilation.GetTypeByMetadataName(typeof(IResolvable).FullName);
            var collectionResolvable = context.Compilation.GetTypeByMetadataName(typeof(ICollectionResolvable).FullName);

            bool IsDerivedFromService(INamedTypeSymbol symbol, INamedTypeSymbol service)
            {
                return symbol.Equals(service)
                    || symbol.AllInterfaces.Any(i => i.Equals(service));
            }

            var isComponent = baseSymbols.OfType<INamedTypeSymbol>()
                                         .Any(symbol => IsDerivedFromService(symbol, resolvable)
                                                   || IsDerivedFromService(symbol, collectionResolvable));

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
    }
}
