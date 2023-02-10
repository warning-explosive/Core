namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using Api.Model;
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

        public void Store(IUniqueIdentified entity)
        {
            var inner = _store.GetOrAdd(entity.GetType(), _ => new ConcurrentDictionary<object, object>());
            _ = inner.GetOrAdd<object>(entity.PrimaryKey, static (_, entity) => entity, entity);
        }

        public IEnumerable<TEntity> GetValues<TEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : IUniqueIdentified
        {
            if (_store.TryGetValue(typeof(TEntity), out var inner))
            {
                return inner.Values.OfType<TEntity>();
            }

            return Enumerable.Empty<TEntity>();
        }

        public bool TryGetValue<TEntity>(object key, [NotNullWhen(true)] out TEntity? entity)
            where TEntity : IUniqueIdentified
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

        public bool TryRemove<TEntity>(object key, [NotNullWhen(true)] out TEntity? entity)
            where TEntity : IUniqueIdentified
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