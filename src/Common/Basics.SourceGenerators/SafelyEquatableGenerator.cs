namespace SpaceEngineers.Core.Basics.SourceGenerators;

using Microsoft.CodeAnalysis;

[Generator]
public class SafelyEquatableGenerator : BasePartialTypeGenerator
{
    protected override bool ImplementsInterface(
        INamedTypeSymbol classOrRecordSymbol,
        out INamedTypeSymbol? interfaceSymbol)
    {
        // implements ISafelyEquatable`1
        interfaceSymbol = classOrRecordSymbol
            .AllInterfaces
            .FirstOrDefault(it => it.Name == "ISafelyEquatable"
                                  && it.Arity == 1
                                  && it.ContainingNamespace.ToDisplayString() == "SpaceEngineers.Core.Basics");

        return interfaceSymbol != null;
    }

    protected override string Generate(
        string @namespace,
        string className,
        string classOrRecord)
    {
        return $$"""
                 #nullable enable

                 namespace {{@namespace}}
                 {
                     public partial {{classOrRecord}} {{className}}
                     {
                         public static bool operator ==({{className}}? left, {{className}}? right)
                         {
                             return Equatable.Equals(left, right);
                         }

                         public static bool operator !=({{className}}? left, {{className}}? right)
                         {
                             return !Equatable.Equals(left, right);
                         }

                         public bool Equals({{className}}? other)
                         {
                             return Equatable.Equals(this, other);
                         }

                         public override bool Equals(object? obj)
                         {
                             return Equatable.Equals(this, obj);
                         }
                     }
                 }
                 """;
    }
}
