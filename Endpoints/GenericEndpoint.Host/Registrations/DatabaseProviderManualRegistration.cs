namespace SpaceEngineers.Core.GenericEndpoint.Host.Registrations
{
    using CompositionRoot.Registration;
    using SpaceEngineers.Core.DataAccess.Orm.Connection;

    internal class DatabaseProviderManualRegistration : IManualRegistration
    {
        private readonly IDatabaseProvider _databaseProvider;

        public DatabaseProviderManualRegistration(IDatabaseProvider databaseProvider)
        {
            _databaseProvider = databaseProvider;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance<IDatabaseProvider>(_databaseProvider);
        }
    }
}