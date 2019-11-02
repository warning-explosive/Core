namespace SpaceEngineers.Core.Utilities.CompositionInfoExtractor
{
    using System;
    using System.Linq;
    using System.Text;
    using CompositionRoot;
    using CompositionRoot.Attributes;
    using CompositionRoot.Enumerations;
    using CompositionRoot.Extensions;

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

        private void VisualizeDependency(DependencyInfo nodeInfo, StringBuilder builder)
        {
            builder.AppendLine(DependencyAsString(nodeInfo));
        }

        private string DependencyAsString(DependencyInfo dependencyInfo)
        {
            return Tabulation((int)dependencyInfo.Depth)
                   + (dependencyInfo.IsCollectionResolvable
                          ? "COLLECTION:"
                          : string.Empty)
                   + TrimGenerics(dependencyInfo.ComponentType)
                   + Generics(dependencyInfo.ComponentType);
        }

        private string Tabulation(int count)
        {
            return new string('\t', count);
        }

        private string TrimGenerics(Type type)
        {
            return type.IsGenericType
                       ? type.Name.Substring(0, type.Name.Length - 2)
                       : type.Name;
        }

        private string Generics(Type type)
        {
            if (!type.IsGenericType)
            {
                return string.Empty;
            }

            const string format = "[{0}]";

            var genericArguments = type.GetGenericTypeDefinition()
                                       .GetGenericArguments();

            return string.Format(format,
                                 genericArguments.Length == 1 ? "T" : string.Join(", ", genericArguments.Select((t, i) => t.Name)));
        }
    }
}