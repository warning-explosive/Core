namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Database
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class InMemoryDatabase : IInMemoryDatabase
    {
        private readonly ConcurrentDictionary<Type, IQueryable> _collections
            = new ConcurrentDictionary<Type, IQueryable>();

        public IQueryable All(Type itemType)
        {
            return _collections.GetOrAdd(itemType, EmptyCollection);
        }

        public IQueryable<T> All<T>()
        {
            return (IQueryable<T>)_collections.GetOrAdd(typeof(T), EmptyCollection);
        }

        private static IQueryable EmptyCollection(Type itemType)
        {
            return typeof(InMemoryDatabase)
                .CallMethod(nameof(EmptyCollection))
                .WithTypeArgument(itemType)
                .Invoke<IQueryable>();
        }

        private static IQueryable<T> EmptyCollection<T>()
        {
            return new EnumerableQuery<T>(Enumerable.Empty<T>());
        }
    }
}