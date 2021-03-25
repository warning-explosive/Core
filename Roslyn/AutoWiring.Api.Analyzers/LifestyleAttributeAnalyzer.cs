namespace SpaceEngineers.Core.AutoWiring.Api.Analyzers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Abstractions;
    using Attributes;
    using Basics.Roslyn;
    using Enumerations;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Component must be marked with LifestyleAttribute
    /// </summary>
    [Lifestyle(EnLifestyle.Transient)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LifestyleAttributeAnalyzer : SyntaxAnalyzerBase
    {
        /// <summary>
        /// MarkWithLifestyleAttribute message
        /// </summary>
        public string MarkWithLifestyleAttribute { get; } =
            $"Mark component with {nameof(LifestyleAttribute)} and select lifestyle";

        /// <inheritdoc />
        public override string Identifier { get; } = "CR1";

        /// <inheritdoc />
        public override string Title { get; } = $"Component must be marked with {nameof(LifestyleAttribute)}";

        /// <inheritdoc />
        public override string Message { get; } = "{0}";

        /// <inheritdoc />
        public override string Category { get; } = "DI Configuration";

        /// <inheritdoc />
        protected override SyntaxKind SyntaxKind { get; } = SyntaxKind.ClassDeclaration;

        /// <inheritdoc />
        [SuppressMessage("Microsoft.CodeAnalysis.Analyzers", "RS1024", Justification = "Error in SymbolEqualityComparer")]
        protected override void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            var isAbstractOrStatic = classDeclarationSyntax
                .Modifiers
                .Any(z => z.IsKind(SyntaxKind.AbstractKeyword)
                          || z.IsKind(SyntaxKind.StaticKeyword));

            if (isAbstractOrStatic)
            {
                return;
            }

            if (!IsComponent(context, classDeclarationSyntax))
            {
                return;
            }

            if (!IsContainsAttribute<LifestyleAttribute>(context, classDeclarationSyntax))
            {
                ReportDiagnostic(context, classDeclarationSyntax.Identifier.GetLocation(), MarkWithLifestyleAttribute);
            }
        }

        [SuppressMessage("Microsoft.CodeAnalysis.Analyzers", "RS1024", Justification = "Error in SymbolEqualityComparer")]
        private static bool IsComponent(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclarationSyntax)
        {
            var baseTypes = classDeclarationSyntax
                .ChildNodes()
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
            var collectionResolvable = context.Compilation.GetTypeByMetadataName(typeof(ICollectionResolvable<>).FullName);
            var externalResolvable = context.Compilation.GetTypeByMetadataName(typeof(IExternalResolvable<>).FullName);
            var decorator = context.Compilation.GetTypeByMetadataName(typeof(IDecorator<>).FullName);
            var conditionalDecorator = context.Compilation.GetTypeByMetadataName(typeof(IConditionalDecorator<,>).FullName);
            var collectionDecorator = context.Compilation.GetTypeByMetadataName(typeof(ICollectionDecorator<>).FullName);
            var conditionalCollectionDecorator = context.Compilation.GetTypeByMetadataName(typeof(IConditionalCollectionDecorator<,>).FullName);

            bool IsDerivedFromService(INamedTypeSymbol symbol, INamedTypeSymbol service)
            {
                return symbol.OriginalDefinition.Equals(service)
                    || symbol.AllInterfaces.Any(i => i.OriginalDefinition.Equals(service));
            }

            var isComponent = baseSymbols
                             .OfType<INamedTypeSymbol>()
                             .Any(symbol => (resolvable != null && IsDerivedFromService(symbol, resolvable))
                                         || (collectionResolvable != null && IsDerivedFromService(symbol, collectionResolvable))
                                         || (externalResolvable != null && IsDerivedFromService(symbol, externalResolvable))
                                         || (decorator != null && IsDerivedFromService(symbol, decorator))
                                         || (conditionalDecorator != null && IsDerivedFromService(symbol, conditionalDecorator))
                                         || (collectionDecorator != null && IsDerivedFromService(symbol, collectionDecorator))
                                         || (conditionalCollectionDecorator != null && IsDerivedFromService(symbol, conditionalCollectionDecorator)));

            return isComponent;
        }

        [SuppressMessage("Microsoft.CodeAnalysis.Analyzers", "RS1024", Justification = "Error in SymbolEqualityComparer")]
        private static bool IsContainsAttribute<TAttribute>(
            SyntaxNodeAnalysisContext context,
            ClassDeclarationSyntax classDeclarationSyntax)
            where TAttribute : Attribute
        {
            var attributeList = classDeclarationSyntax.ChildNodes().OfType<AttributeListSyntax>().ToArray();

            if (!attributeList.Any())
            {
                return false;
            }

            var attribute = context.Compilation.GetTypeByMetadataName(typeof(TAttribute).FullName);

            var isContainsAttribute = attributeList
                .SelectMany(z => z.Attributes)
                .Select(GetAttributeSymbol)
                .Any(z => z.Equals(attribute));

            return isContainsAttribute;

            ITypeSymbol? GetAttributeSymbol(AttributeSyntax syntax)
            {
                return context.Compilation
                              .GetSemanticModel(syntax.SyntaxTree)
                              .GetTypeInfo(syntax)
                              .Type;
            }
        }
    }
}
