namespace SpaceEngineers.Core.DataAccess.Orm.Host.ManualRegistrations
{
    using System.Collections.Generic;
    using Api.Model;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;

    internal class DatabaseTypeProviderManualRegistration : IManualRegistration
    {
        private readonly IReadOnlyCollection<IEndpointDatabaseTypeProvider> _endpointDatabaseTypeProviders;

        public DatabaseTypeProviderManualRegistration(IReadOnlyCollection<IEndpointDatabaseTypeProvider> endpointDatabaseTypeProviders)
        {
            _endpointDatabaseTypeProviders = endpointDatabaseTypeProviders;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            foreach (var provider in _endpointDatabaseTypeProviders)
            {
                container.RegisterCollectionEntryInstance<IEndpointDatabaseTypeProvider>(provider);
            }

            container.RegisterDecorator<IDatabaseTypeProvider, MigrationsDatabaseTypeProviderDecorator>(EnLifestyle.Singleton);
        }
    }
}