namespace SpaceEngineers.Core.DataAccess.Orm.Host.Registrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;
    using SpaceEngineers.Core.DataAccess.Api.Model;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;

    internal class DatabaseTypeProviderManualRegistration : IManualRegistration
    {
        private readonly IFrameworkDependenciesProvider _frameworkDependenciesProvider;
        private readonly IReadOnlyCollection<Func<IFrameworkDependenciesProvider, IDependencyContainer>> _producers;

        public DatabaseTypeProviderManualRegistration(
            IFrameworkDependenciesProvider frameworkDependenciesProvider,
            IReadOnlyCollection<Func<IFrameworkDependenciesProvider, IDependencyContainer>> producers)
        {
            _frameworkDependenciesProvider = frameworkDependenciesProvider;
            _producers = producers;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            foreach (var producer in _producers)
            {
                container.Advanced.RegisterCollectionEntryDelegate<IEndpointDatabaseTypeProvider>(() =>
                        new EndpointDatabaseTypeProvider(producer(_frameworkDependenciesProvider)
                           .Resolve<IDatabaseTypeProvider>()
                           .DatabaseEntities()
                           .ToList()),
                    EnLifestyle.Singleton);
            }

            container.RegisterDecorator<IDatabaseTypeProvider, MigrationsDatabaseTypeProviderDecorator>(EnLifestyle.Singleton);
        }
    }
}