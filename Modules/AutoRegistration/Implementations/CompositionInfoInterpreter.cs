namespace SpaceEngineers.Core.AutoRegistration.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Contexts;
    using AutoWiringApi.Enumerations;
    using AutoWiringApi.Services;
    using Basics;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class CompositionInfoInterpreter : ICompositionInfoInterpreter<string>
    {
        /// <inheritdoc />
        public string Visualize(IReadOnlyCollection<IDependencyInfo> compositionInfo)
        {
            var builder = new StringBuilder();

            compositionInfo.Each(dependencyInfo => dependencyInfo.TraverseByGraph(di => VisualizeDependency(di, builder)));

            return builder.ToString();
        }

        private static void VisualizeDependency(IDependencyInfo nodeInfo, StringBuilder builder)
        {
            builder.AppendLine(DependencyAsString(nodeInfo));
        }

        private static string DependencyAsString(IDependencyInfo dependencyInfo)
        {
            return Tabulation((int)dependencyInfo.Depth)
                 + Tags(dependencyInfo)
                 + TrimGenerics(dependencyInfo.ImplementationType)
                 + Generics(dependencyInfo.ImplementationType);
        }

        private static string Tabulation(int count)
        {
            return new string('\t', count);
        }

        private static string Tags(IDependencyInfo dependencyInfo)
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

            if (dependencyInfo.ServiceType == dependencyInfo.ImplementationType)
            {
                tags.Add("[IMPLEMENTATION]");
            }

            tags.Add(Lifestyle(dependencyInfo.Lifestyle));

            return string.Join(string.Empty, tags);
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

            return string.Format(CultureInfo.InvariantCulture,
                                 format,
                                 string.Join(", ", genericArguments.Select((t, i) => t.Name)));
        }
    }
}