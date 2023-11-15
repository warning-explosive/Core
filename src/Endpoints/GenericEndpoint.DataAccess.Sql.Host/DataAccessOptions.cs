namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host
{
    using Registrations;
    using SpaceEngineers.Core.GenericEndpoint.Host.Builder;

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
            _ = _endpointBuilder
               .ModifyContainerOptions(options => options
                   .WithManualRegistrations(new UpgradeDatabaseHostStartupActionManualRegistration()));

            return this;
        }
    }
}