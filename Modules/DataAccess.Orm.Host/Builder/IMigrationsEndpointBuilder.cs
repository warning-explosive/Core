namespace SpaceEngineers.Core.DataAccess.Orm.Host.Builder
{
    using System;
    using System.Reflection;
    using CompositionRoot;
    using Connection;

    /// <summary>
    /// IMigrationsEndpointBuilder
    /// </summary>
    public interface IMigrationsEndpointBuilder
    {
        /// <summary>
        /// With data access
        /// </summary>
        /// <param name="databaseProvider">Database provider</param>
        /// <returns>IMigrationsEndpointBuilder</returns>
        IMigrationsEndpointBuilder WithDataAccess(IDatabaseProvider databaseProvider);

        /// <summary>
        /// With endpoint plugin assemblies
        /// </summary>
        /// <param name="assemblies">Endpoint plugin assemblies</param>
        /// <returns>IMigrationsEndpointBuilder</returns>
        public IMigrationsEndpointBuilder WithEndpointPluginAssemblies(params Assembly[] assemblies);

        /// <summary>
        /// Modify container options
        /// </summary>
        /// <param name="modifier">Modifier</param>
        /// <returns>IMigrationsEndpointBuilder</returns>
        IMigrationsEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier);

        /// <summary>
        /// Build transport endpoint options
        /// </summary>
        /// <returns>IMigrationsEndpointBuilder</returns>
        MigrationsEndpointOptions BuildOptions();
    }
}