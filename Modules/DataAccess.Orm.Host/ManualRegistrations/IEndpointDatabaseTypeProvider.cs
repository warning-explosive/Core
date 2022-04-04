namespace SpaceEngineers.Core.DataAccess.Orm.Host.ManualRegistrations
{
    using System;
    using System.Collections.Generic;

    internal interface IEndpointDatabaseTypeProvider
    {
        IEnumerable<Type> DatabaseEntities();
    }
}