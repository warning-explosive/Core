namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using System.Data;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Sql.Settings;
    using Mocks;

    internal class IsolationLevelManualRegistration : IManualRegistration
    {
        private readonly IsolationLevel _isolationLevel;

        public IsolationLevelManualRegistration(IsolationLevel isolationLevel)
        {
            _isolationLevel = isolationLevel;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(new TestSqlDatabaseSettingsProviderDecorator.IsolationLevelProvider(_isolationLevel));
            container.RegisterDecorator<ISettingsProvider<SqlDatabaseSettings>, TestSqlDatabaseSettingsProviderDecorator>(EnLifestyle.Singleton);
        }
    }
}