using Xunit;

[assembly: TestCollectionOrderer("SpaceEngineers.Core.Roslyn.Test.Tests.DisplayNameTestCollectionOrderer", "SpaceEngineers.Core.Roslyn.Test")]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit.Abstractions;

    /// <summary>
    /// DisplayNameTestCollectionOrderer
    /// </summary>
    public class DisplayNameTestCollectionOrderer : ITestCollectionOrderer
    {
        /// <inheritdoc />
        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            return testCollections.OrderBy(collection => collection.DisplayName);
        }
    }
}