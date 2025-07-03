namespace SpaceEngineers.Core.Basics.SourceGenerators;

using System.Text;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

public abstract class BasePartialTypeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var recordDeclarations = context
            .SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is RecordDeclarationSyntax recordDeclaration
                    && recordDeclaration.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)),
                transform: static (ctx, _) => (TypeDeclarationSyntax)ctx.Node);

        var recordsSource = context.CompilationProvider.Combine(recordDeclarations.Collect());

        context.RegisterSourceOutput(recordsSource, Generate);

        var classDeclarations = context
            .SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is ClassDeclarationSyntax classDeclaration
                    && classDeclaration.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)),
                transform: static (ctx, _) => (TypeDeclarationSyntax)ctx.Node);

        var classesSource = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(classesSource, Generate);
    }

    protected abstract bool ImplementsInterface(
        INamedTypeSymbol classOrRecordSymbol,
        out INamedTypeSymbol? interfaceSymbol);

    protected abstract string Generate(
        string @namespace,
        string className,
        string classOrRecord);

    private void Generate(
        SourceProductionContext context,
        (Compilation, ImmutableArray<TypeDeclarationSyntax>) source)
    {
        var (compilation, declarations) = source;

        foreach (var declaration in declarations)
        {
            var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
            var symbol = semanticModel.GetDeclaredSymbol(declaration);

            if (symbol is not INamedTypeSymbol namedTypeSymbol)
            {
                continue;
            }

            Generate(context, declaration, namedTypeSymbol);
        }
    }

    private void Generate(
        SourceProductionContext context,
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol classOrRecordSymbol)
    {
        if (!ImplementsInterface(classOrRecordSymbol, out var interfaceSymbol))
        {
            return;
        }

        var @namespace = classOrRecordSymbol.ContainingNamespace.ToDisplayString();
        var className = GetDisplayName(classOrRecordSymbol);
        var generated = syntax switch
        {
            RecordDeclarationSyntax => Generate(@namespace, className, "record"),
            ClassDeclarationSyntax => Generate(@namespace, className, "class"),
            _ => throw new NotSupportedException(syntax.GetType().ToString())
        };

        context.AddSource(
            $"{classOrRecordSymbol.Name}_{interfaceSymbol!.Name}_{interfaceSymbol.Arity}.g.cs",
            SourceText.From(generated, Encoding.UTF8));
    }

    private static string GetDisplayName(INamedTypeSymbol symbol)
    {
        var sb = new StringBuilder();

        sb.Append(symbol.Name);

        if (symbol.IsGenericType)
        {
            var typeArguments = string.Join(", ", symbol.TypeArguments.Select(it => it.Name));
            sb.Append("<");
            sb.Append(typeArguments);
            sb.Append(">");
        }

        return sb.ToString();
    }
}