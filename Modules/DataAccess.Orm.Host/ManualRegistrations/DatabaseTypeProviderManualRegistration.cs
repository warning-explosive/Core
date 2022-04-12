namespace SpaceEngineers.Core.DataAccess.Orm.Host.ManualRegistrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Model;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.Api.Abstractions.Registration;
    using GenericHost.Api.Abstractions;

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