namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Host
{
    using System;
    using Basics;
    using GenericEndpoint.Host.Builder;
    using Overrides;
    using Registrations;

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
            var dataAccessAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(DataAccess)));

            var postgreSqlDataAccess = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.PostgreSql))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Dynamic))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Host))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql), nameof(Core.DataAccess.Orm.Sql.Host))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.PostgreSql), nameof(Core.DataAccess.Orm.PostgreSql.Host)))
            };

            builder
               .WithEndpointPluginAssemblies(dataAccessAssembly)
               .WithEndpointPluginAssemblies(postgreSqlDataAccess)
               .ModifyContainerOptions(options => options
                   .WithManualRegistrations(new GenericEndpointDataAccessHostBackgroundWorkerManualRegistration())
                   .WithOverrides(new DataAccessOverride()));

            dataAccessOptions?.Invoke(new DataAccessOptions(builder));

            return builder;
        }

        /// <summary>
        /// With sql event sourcing
        /// </summary>
        /// <param name="builder">Endpoint builder</param>
        /// <returns>IEndpointBuilder</returns>
        public static IEndpointBuilder WithSqlEventSourcing(
            this IEndpointBuilder builder)
        {
            var sqlEventSourcing = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericDomain), nameof(Core.GenericDomain.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericDomain), nameof(Core.GenericDomain.EventSourcing))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericDomain), nameof(Core.GenericDomain.EventSourcing), nameof(Core.GenericDomain.EventSourcing.Sql)))
            };

            return builder.WithEndpointPluginAssemblies(sqlEventSourcing);
        }
    }
}