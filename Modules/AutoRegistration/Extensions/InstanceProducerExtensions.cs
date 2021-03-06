namespace SpaceEngineers.Core.AutoRegistration.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Internals;
    using SimpleInjector;

    internal static class InstanceProducerExtensions
    {
        internal static IOrderedEnumerable<InstanceProducer> OrderByComplexityDepth(this IEnumerable<InstanceProducer> source)
        {
            return source.OrderBy(producer => DependencyInfo.RetrieveDependencyGraph(producer).ComplexityDepth);
        }
    }
}