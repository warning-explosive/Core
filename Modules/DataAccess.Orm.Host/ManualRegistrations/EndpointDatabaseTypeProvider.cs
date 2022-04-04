namespace SpaceEngineers.Core.DataAccess.Orm.Host.ManualRegistrations
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;

    [UnregisteredComponent] // ("It is created manually and registered as delegate so as to perform intersection between different database models")
    internal class EndpointDatabaseTypeProvider : IEndpointDatabaseTypeProvider,
                                                  ICollectionResolvable<IEndpointDatabaseTypeProvider>
    {
        private readonly IReadOnlyCollection<Type> _databaseEntities;

        public EndpointDatabaseTypeProvider(IReadOnlyCollection<Type> databaseEntities)
        {
            _databaseEntities = databaseEntities;
        }

        public IEnumerable<Type> DatabaseEntities()
        {
            return _databaseEntities;
        }
    }
}