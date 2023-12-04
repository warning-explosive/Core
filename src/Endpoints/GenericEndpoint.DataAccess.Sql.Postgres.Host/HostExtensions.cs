namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Postgres.Host
{
    using System;
    using Basics;
    using Registrations;
    using SpaceEngineers.Core.GenericEndpoint.Host.Builder;
    using Sql.Host;
    using Sql.Host.Overrides;
    using Translation;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// With PostgreSql data access
        /// </summary>
        /// <param name="builder">IEndpointBuilder</param>
        /// <param name="dataAccessOptions">Data access options</param>
        /// <returns>Configured IEndpointBuilder</returns>
        public static IEndpointBuilder WithPostgreSqlDataAccess(
            this IEndpointBuilder builder,
            Action<DataAccessOptions>? dataAccessOptions)
        {
            builder.CheckMultipleCalls(nameof(WithPostgreSqlDataAccess));

            var postgreSqlDataAccess = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(DataAccess), nameof(Sql))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Dynamic))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql), nameof(Core.DataAccess.Orm.Sql.Postgres))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql), nameof(Core.DataAccess.Orm.Sql.Migrations))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql), nameof(Core.DataAccess.Orm.Sql.Migrations), nameof(Core.DataAccess.Orm.Sql.Migrations.Postgres)))
            };

            builder.ModifyContainerOptions(options => options
                .WithPluginAssemblies(postgreSqlDataAccess)
                .WithAdditionalOurTypes(typeof(QuerySourceExpressionTranslator))
                .WithManualRegistrations(new DataAccessHostManualRegistration())
                .WithOverrides(new DataAccessOverride()));

            dataAccessOptions?.Invoke(new DataAccessOptions(builder));

            return builder;
        }
    }
}