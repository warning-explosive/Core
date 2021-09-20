namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Database
{
    using System;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;

    internal interface IInMemoryDatabase : IResolvable
    {
        IQueryable All(Type itemType);

        IQueryable<T> All<T>();
    }
}