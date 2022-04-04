namespace SpaceEngineers.Core.DataAccess.Orm.Host.ManualRegistrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Model;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class MigrationsDatabaseTypeProviderDecorator : IDatabaseTypeProvider,
                                                             IDecorator<IDatabaseTypeProvider>
    {
        private readonly IEnumerable<IEndpointDatabaseTypeProvider> _endpointDatabaseTypeProviders;

        public MigrationsDatabaseTypeProviderDecorator(
            IDatabaseTypeProvider decoratee,
            IEnumerable<IEndpointDatabaseTypeProvider> endpointDatabaseTypeProviders)
        {
            Decoratee = decoratee;
            _endpointDatabaseTypeProviders = endpointDatabaseTypeProviders;
        }

        public IDatabaseTypeProvider Decoratee { get; }

        public IEnumerable<Type> DatabaseEntities()
        {
            return Decoratee
               .DatabaseEntities()
               .Concat(_endpointDatabaseTypeProviders
                   .SelectMany(provider => provider.DatabaseEntities()))
               .Distinct();
        }
    }
}