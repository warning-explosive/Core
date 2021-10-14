namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Migrations
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ModelChangesSorter : IModelChangesSorter
    {
        public IOrderedEnumerable<IModelChange> Sort(IEnumerable<IModelChange> source)
        {
            return source.AsOrderedEnumerable();
        }
    }
}