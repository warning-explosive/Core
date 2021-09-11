namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase
{
    using System.Collections.Generic;
    using System.Reflection;
    using Basics;

    /// <summary>
    /// InMemoryDatabaseProvider
    /// </summary>
    public class InMemoryDatabaseProvider : IDatabaseProvider
    {
        /// <inheritdoc />
        public IEnumerable<Assembly> Implementation()
        {
            return new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.InMemoryDatabase)))
            };
        }
    }
}