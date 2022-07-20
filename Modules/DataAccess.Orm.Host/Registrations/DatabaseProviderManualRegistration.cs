namespace SpaceEngineers.Core.DataAccess.Orm.Host.Registrations
{
    using CompositionRoot.Api.Abstractions.Registration;
    using Connection;

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