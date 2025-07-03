namespace SpaceEngineers.Core.Basics.SourceGenerators;

using Microsoft.CodeAnalysis;

[Generator]
public class SafelyComparableGenerator : BasePartialTypeGenerator
{
    protected override bool ImplementsInterface(
        INamedTypeSymbol classOrRecordSymbol,
        out INamedTypeSymbol? interfaceSymbol)
    {
        // implements ISafelyEquatable`1
        interfaceSymbol = classOrRecordSymbol
            .AllInterfaces
            .FirstOrDefault(it => it.Name == "ISafelyComparable"
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
                          public static bool operator <({{className}}? left, {{className}}? right)
                          {
                              return Comparable.Less(left, right);
                          }

                          public static bool operator >({{className}}? left, {{className}}? right)
                          {
                              return Comparable.Greater(left, right);
                          }

                          public static bool operator <=({{className}}? left, {{className}}? right)
                          {
                              return Comparable.LessOrEquals(left, right);
                          }

                          public static bool operator >=({{className}}? left, {{className}}? right)
                          {
                              return Comparable.GreaterOrEquals(left, right);
                          }

                          public int CompareTo({{className}}? other)
                          {
                              return Comparable.CompareTo(this, other);
                          }

                          public int CompareTo(object? obj)
                          {
                              return Comparable.CompareTo(this, obj);
                          }
                     }
                 }
                 """;
    }
}
