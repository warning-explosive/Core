namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;

    internal interface IDatabaseTypeProvider
    {
        IEnumerable<Type> DatabaseEntities();
    }
}