namespace SpaceEngineers.Core.DataAccess.PostgreSql.Host
{
    using System.Collections.Generic;
    using System.Reflection;
    using Basics;
    using GenericEndpoint.Host.Abstractions;

    /// <summary>
    /// PostgreSqlDatabaseProvider
    /// </summary>
    public class PostgreSqlDatabaseProvider : IDatabaseProvider
    {
        /// <inheritdoc />
        public IEnumerable<Assembly> Implementation()
        {
            return new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.PostgreSql)))
            };
        }
    }
}