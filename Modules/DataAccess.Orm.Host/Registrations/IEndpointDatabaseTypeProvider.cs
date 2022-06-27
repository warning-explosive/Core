namespace SpaceEngineers.Core.DataAccess.Orm.Host.Registrations
{
    using System;
    using System.Collections.Generic;

    internal interface IEndpointDatabaseTypeProvider
    {
        IEnumerable<Type> DatabaseEntities();
    }
}