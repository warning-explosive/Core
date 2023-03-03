namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System.Data;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Sql.Settings;

    [ManuallyRegisteredComponent(nameof(TranslationTest))]
    internal class TestSqlDatabaseSettingsProviderDecorator : ISettingsProvider<SqlDatabaseSettings>,
                                                              IDecorator<ISettingsProvider<SqlDatabaseSettings>>
    {
        private readonly IsolationLevelProvider _isolationLevelProvider;

        public TestSqlDatabaseSettingsProviderDecorator(
            ISettingsProvider<SqlDatabaseSettings> decoratee,
            IsolationLevelProvider isolationLevelProvider)
        {
            Decoratee = decoratee;
            _isolationLevelProvider = isolationLevelProvider;
        }

        public ISettingsProvider<SqlDatabaseSettings> Decoratee { get; }

        public SqlDatabaseSettings Get()
        {
            var sqlDatabaseSettings = Decoratee.Get();

            typeof(SqlDatabaseSettings)
                .GetProperty(nameof(SqlDatabaseSettings.IsolationLevel), BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
                .SetValue(sqlDatabaseSettings, _isolationLevelProvider.IsolationLevel);

            return sqlDatabaseSettings;
        }

        [ManuallyRegisteredComponent(nameof(TranslationTest))]
        internal class IsolationLevelProvider : IResolvable<IsolationLevelProvider>
        {
            public IsolationLevelProvider(IsolationLevel isolationLevel)
            {
                IsolationLevel = isolationLevel;
            }

            public IsolationLevel IsolationLevel { get; }
        }
    }
}