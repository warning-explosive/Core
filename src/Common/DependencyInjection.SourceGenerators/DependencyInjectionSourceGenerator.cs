namespace SpaceEngineers.Core.DependencyInjection.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
[SuppressMessage("Analysis", "RS1041", Justification = "DI SourceGenerator")]
[SuppressMessage("Analysis", "RS1035", Justification = "DI SourceGenerator")]
public class DependencyInjectionSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var componentAttributeSymbol = context
            .Compilation
            .GetTypeByMetadataName("SpaceEngineers.Core.DependencyInjection.ComponentAttribute");

        var components = context
            .Compilation
            .SyntaxTrees
            .Where(syntaxTree => syntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Any())
            .SelectMany(syntaxTree => syntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>())
            .Where(it => it
                .DescendantNodes()
                .OfType<AttributeSyntax>()
                .Any())
            .Select(classDeclarationSyntax =>
            {
                var componentAttributeSyntax = classDeclarationSyntax
                    .DescendantNodes()
                    .OfType<AttributeSyntax>()
                    .Where(attributeSyntax =>
                    {
                        var attributeSymbol = context
                            .Compilation
                            .GetSemanticModel(attributeSyntax.SyntaxTree)
                            .GetTypeInfo(attributeSyntax)
                            .Type;

                        return IsTheSameType((INamedTypeSymbol)attributeSymbol!, componentAttributeSymbol!);
                    })
                    .SingleOrDefault();

                return (classDeclarationSyntax, componentAttributeSyntax);
            })
            .Where(it => it.componentAttributeSyntax != null)
            .Select(it =>
            {
                var (classDeclarationSyntax, componentAttributeSyntax) = it;

                var type = context
                    .Compilation
                    .GetSemanticModel(classDeclarationSyntax.SyntaxTree)
                    .GetDeclaredSymbol(classDeclarationSyntax) !;

                var objectTypeSymbol = context.Compilation.GetTypeByMetadataName(typeof(object).FullName!) !;

                var includedTypes = IncludedTypes(type)
                    .Where(includedType => !IsTheSameType(includedType, objectTypeSymbol))
                    .ToList();

                var lifestyleSyntax = componentAttributeSyntax!
                    .ArgumentList
                    .Arguments[0]
                    .Expression;

                var lifestyleSymbol = (IFieldSymbol)context
                    .Compilation
                    .GetSemanticModel(lifestyleSyntax.SyntaxTree)
                    .GetSymbolInfo(lifestyleSyntax)
                    .Symbol !;

                var lifestyle = lifestyleSymbol.Type.Name + "." + (int)lifestyleSymbol.ConstantValue ! switch
                {
                    0 => "Transient",
                    1 => "Scoped",
                    2 => "Singleton",
                    _ => throw new ArgumentOutOfRangeException(lifestyleSymbol.ConstantValue.ToString())
                };

                return (type, includedTypes, lifestyle);
            })
            .ToList();

        var registrations = components
            .SelectMany(it => it
                .includedTypes
                .Select(includedType =>
                {
                    var isDecorator = it
                        .type
                        .Constructors
                        .Any(cctor => cctor
                            .Parameters
                            .Any(parameter => IsTheSameType((INamedTypeSymbol)parameter.Type, includedType)));

                    return (it.type, includedType, it.lifestyle, isDecorator);
                }))
            .OrderBy(it => it.isDecorator)
            .Select(it =>
            {
                var (type, includedType, lifestyle, isDecorator) = it;
                {
                    return isDecorator
                        ? $"/*decorator*/ container.RegisterDecorator(typeof({GetTypeName(includedType)}), typeof({GetTypeName(type)}), {lifestyle});"
                        : $"container.Register(typeof({GetTypeName(includedType)}), typeof({GetTypeName(type)}), {lifestyle});";
                }
            });

        var rootNamespace = components.First().type.ContainingAssembly.Name;

        var sourceText = $@"namespace {rootNamespace};

using System.Runtime.CompilerServices;
using SpaceEngineers.Core.DependencyInjection.Registration;
using SpaceEngineers.Core.DependencyInjection;

[CompilerGenerated]
public class GeneratedDependencyInjectionRegistration : IDependencyInjectionRegistration
{{
    public void Register(IDependencyInjectionRegistrationContainer container)
    {{
        {string.Join("\n\t\t", registrations)}
    }}
}}
";

        context.AddSource("GeneratedDependencyInjectionRegistration.g.cs", SourceText.From(sourceText, Encoding.UTF8));
    }

    private static IEnumerable<INamedTypeSymbol> IncludedTypes(ITypeSymbol typeSymbol)
    {
        yield return (INamedTypeSymbol)typeSymbol;

        var baseType = typeSymbol.BaseType;

        while (baseType != null)
        {
            yield return baseType;
            baseType = baseType.BaseType;
        }

        foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
        {
            yield return interfaceSymbol;
        }
    }

    private static bool IsTheSameType(INamedTypeSymbol left, INamedTypeSymbol right)
    {
        return GetTypeName(left).Equals(GetTypeName(right));
    }

    private static string GetTypeName(INamedTypeSymbol typeSymbol)
    {
        var arity = typeSymbol.Arity - 1 >= 0
            ? $"<{new string(',', typeSymbol.Arity - 1)}>"
            : string.Empty;

        var typeName = typeSymbol.ContainingNamespace
                       + "."
                       + typeSymbol.Name
                       + arity;

        return typeName;
    }
}