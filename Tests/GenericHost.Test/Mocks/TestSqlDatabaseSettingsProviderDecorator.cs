namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
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

        public async Task<SqlDatabaseSettings> Get(CancellationToken token)
        {
            var sqlDatabaseSettings = await Decoratee
               .Get(token)
               .ConfigureAwait(false);

            sqlDatabaseSettings.IsolationLevel = _isolationLevelProvider.IsolationLevel;

            return sqlDatabaseSettings;
        }

        [ManuallyRegisteredComponent(nameof(TranslationTest))]
        internal class IsolationLevelProvider : IResolvable<IsolationLevelProvider>
        {
            private readonly IsolationLevel _isolationLevel;

            public IsolationLevelProvider(IsolationLevel isolationLevel)
            {
                _isolationLevel = isolationLevel;
            }

            public IsolationLevel IsolationLevel => _isolationLevel;
        }
    }
}