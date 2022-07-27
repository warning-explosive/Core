namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using Api.Model;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class TransactionalStore : ITransactionalStore,
                                        IResolvable<ITransactionalStore>
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, object>> _store;

        public TransactionalStore()
        {
            _store = new ConcurrentDictionary<Type, ConcurrentDictionary<object, object>>();
        }

        public void Store<TEntity, TKey>(TEntity entity)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            var inner = _store.GetOrAdd(typeof(TEntity), _ => new ConcurrentDictionary<object, object>());
            _ = inner.GetOrAdd<object>(entity.PrimaryKey, static (_, entity) => entity, entity);
        }

        public IEnumerable<TEntity> GetValues<TEntity, TKey>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            if (_store.TryGetValue(typeof(TEntity), out var inner))
            {
                return inner.Values.OfType<TEntity>();
            }

            return Enumerable.Empty<TEntity>();
        }

        public bool TryGetValue<TEntity, TKey>(TKey key, [NotNullWhen(true)] out TEntity? entity)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            if (_store.TryGetValue(typeof(TEntity), out var inner)
                && inner.TryGetValue(key, out var value))
            {
                entity = (TEntity)value;
                return true;
            }

            entity = default;
            return false;
        }

        public bool TryRemove<TEntity, TKey>(TKey key, [NotNullWhen(true)] out TEntity? entity)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            if (_store.TryGetValue(typeof(TEntity), out var inner)
                && inner.TryRemove(key, out var value))
            {
                entity = (TEntity)value;
                return true;
            }

            entity = default;
            return false;
        }

        public void Clear()
        {
            Dispose();
        }

        public void Apply(ITransactionalChange change)
        {
            change.Apply(this);
        }

        public void Dispose()
        {
            foreach (var entity in _store)
            {
                entity.Value.Clear();
            }

            _store.Clear();
        }
    }
}