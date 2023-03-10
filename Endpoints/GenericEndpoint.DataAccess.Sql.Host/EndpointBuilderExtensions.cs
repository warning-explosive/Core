namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host
{
    using System;
    using Basics;
    using Overrides;
    using Registrations;
    using SpaceEngineers.Core.GenericEndpoint.Host.Builder;

    /// <summary>
    /// EndpointBuilderExtensions
    /// </summary>
    public static class EndpointBuilderExtensions
    {
        /// <summary>
        /// With PostgreSql data access
        /// </summary>
        /// <param name="builder">Endpoint builder</param>
        /// <param name="dataAccessOptions">Data access options</param>
        /// <returns>IEndpointBuilder</returns>
        public static IEndpointBuilder WithPostgreSqlDataAccess(
            this IEndpointBuilder builder,
            Action<DataAccessOptions>? dataAccessOptions)
        {
            var postgreSqlDataAccess = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(DataAccess), nameof(DataAccess.Sql))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Dynamic))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.PostgreSql))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql), nameof(Core.DataAccess.Orm.Sql.Host))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.PostgreSql), nameof(Core.DataAccess.Orm.PostgreSql.Host)))
            };

            builder
               .WithEndpointPluginAssemblies(postgreSqlDataAccess)
               .ModifyContainerOptions(options => options
                   .WithManualRegistrations(new DataAccessHostManualRegistration())
                   .WithOverrides(new DataAccessOverride()));

            dataAccessOptions?.Invoke(new DataAccessOptions(builder));

            return builder;
        }
    }
}