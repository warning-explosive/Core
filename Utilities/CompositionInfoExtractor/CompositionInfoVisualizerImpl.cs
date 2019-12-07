namespace SpaceEngineers.Core.CompositionInfoExtractor
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Attributes;
    using CompositionRoot.Enumerations;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class CompositionInfoVisualizerImpl : ICompositionInfoVisualizer
    {
        /// <inheritdoc />
        public string Visualize(DependencyInfo[] compositionInfo)
        {
            var builder = new StringBuilder();

            compositionInfo.Each(dependencyInfo => dependencyInfo.ExecuteAction(di => VisualizeDependency(di, builder)));

            return builder.ToString();
        }

        private static void VisualizeDependency(DependencyInfo nodeInfo, StringBuilder builder)
        {
            builder.AppendLine(DependencyAsString(nodeInfo));
        }

        private static string DependencyAsString(DependencyInfo dependencyInfo)
        {
            return Tabulation((int)dependencyInfo.Depth)
                   + (dependencyInfo.IsCollectionResolvable
                          ? "COLLECTION:"
                          : string.Empty)
                   + TrimGenerics(dependencyInfo.ComponentType)
                   + Generics(dependencyInfo.ComponentType);
        }

        private static string Tabulation(int count)
        {
            return new string('\t', count);
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

            const string format = "[{0}]";

            var genericArguments = type.GetGenericTypeDefinition()
                                       .GetGenericArguments();

            return string.Format(CultureInfo.InvariantCulture,
                                 format,
                                 genericArguments.Length == 1 ? "T" : string.Join(", ", genericArguments.Select((t, i) => t.Name)));
        }
    }
}