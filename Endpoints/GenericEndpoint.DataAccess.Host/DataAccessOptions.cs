namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Host
{
    using Basics;
    using GenericEndpoint.Host.Builder;
    using Registrations;

    /// <summary>
    /// DataAccessOptions
    /// </summary>
    public class DataAccessOptions
    {
        private readonly IEndpointBuilder _endpointBuilder;

        /// <summary> .cctor </summary>
        /// <param name="endpointBuilder">IEndpointBuilder</param>
        public DataAccessOptions(IEndpointBuilder endpointBuilder)
        {
            _endpointBuilder = endpointBuilder;
        }

        /// <summary>
        /// Executes migrations on endpoint startup
        /// </summary>
        /// <returns>IMigrationsEndpointBuilder</returns>
        public DataAccessOptions ExecuteMigrations()
        {
            var assembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Host)));

            _ = _endpointBuilder
               .WithEndpointPluginAssemblies(assembly)
               .ModifyContainerOptions(options => options
                   .WithManualRegistrations(new UpgradeDatabaseHostStartupActionManualRegistration()));

            return this;
        }
    }
}