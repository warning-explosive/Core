namespace SpaceEngineers.Core.DataAccess.Orm.Host.Registrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Api.Model;

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