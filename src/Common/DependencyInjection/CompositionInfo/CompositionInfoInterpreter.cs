namespace SpaceEngineers.Core.DependencyInjection.CompositionInfo;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Basics;

[Component(EnLifestyle.Singleton)]
public class CompositionInfoInterpreter : ICompositionInfoInterpreter<string>
{
    public string Visualize(IReadOnlyCollection<DependencyInfo> compositionInfo)
    {
        var builder = new StringBuilder();

        compositionInfo.Each(dependencyInfo => dependencyInfo.TraverseByGraph(di => VisualizeDependency(di, builder)));

        return builder.ToString();
    }

    private static void VisualizeDependency(DependencyInfo nodeInfo, StringBuilder builder)
    {
        builder.AppendLine(DependencyAsString(nodeInfo));
    }

    private static string DependencyAsString(DependencyInfo dependencyInfo)
    {
        return Tabulation((int)dependencyInfo.Depth)
               + Tags(dependencyInfo)
               + TrimGenerics(dependencyInfo.ImplementationType)
               + Generics(dependencyInfo.ImplementationType)
               + " : "
               + TrimGenerics(dependencyInfo.ServiceType);
    }

    private static string Tabulation(int count)
    {
        return new string('\t', count);
    }

    private static string Tags(DependencyInfo dependencyInfo)
    {
        var tags = new List<string>();

        if (dependencyInfo.IsUnregistered)
        {
            tags.Add("[UNREGISTERED]");
        }

        if (dependencyInfo.ImplementationType.IsGenericType)
        {
            tags.Add("[GENERIC]");
        }

        if (dependencyInfo.IsCollectionResolvable)
        {
            tags.Add("[COLLECTION]");
        }

        if (dependencyInfo.ImplementationType.IsDecorator(dependencyInfo.ServiceType))
        {
            tags.Add("[DECORATOR]");
        }

        if (dependencyInfo.ServiceType == dependencyInfo.ImplementationType)
        {
            tags.Add("[IMPLEMENTATION]");
        }
        else
        {
            tags.Add("[SERVICE]");
        }

        tags.Add(Lifestyle(dependencyInfo.Lifestyle));

        return tags.ToString(string.Empty);
    }

    private static string Lifestyle(EnLifestyle? lifestyle)
    {
        return $"[{(lifestyle?.ToString() ?? "UNSUPPORTED").ToUpperInvariant()}]";
    }

    private static string TrimGenerics(Type type)
    {
        return type.IsGenericType
            ? type.Name.Substring(0, type.Name.Length - 2)
            : type.Name;
    }

    private static string Generics(Type type)
    {
        if (!type.IsGenericType)
        {
            return string.Empty;
        }

        const string format = "<{0}>";

        var genericArguments = type.GetGenericTypeDefinition().GetGenericArguments();

        return format.Format(genericArguments.Select((t, i) => t.Name).ToString(", "));
    }
}