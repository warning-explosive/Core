namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
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

        public void Store<TEntry, TKey>(TEntry obj, Func<TEntry, TKey> keySelector)
            where TEntry : notnull
            where TKey : notnull
        {
            var inner = _store.GetOrAdd(typeof(TEntry), _ => new ConcurrentDictionary<object, object>());
            _ = inner.GetOrAdd<object>(keySelector(obj), static (_, o) => o, obj);
        }

        public bool TryGetValue<TEntry, TKey>(TKey key, [NotNullWhen(true)] out TEntry? entry)
            where TEntry : notnull
            where TKey : notnull
        {
            if (_store.TryGetValue(typeof(TEntry), out var inner)
                && inner.TryGetValue(key, out var value))
            {
                entry = (TEntry)value;
                return true;
            }

            entry = default;
            return false;
        }

        public void Invalidate<TEntry, TKey>(TKey key)
            where TEntry : notnull
            where TKey : notnull
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            foreach (var entry in _store)
            {
                entry.Value.Clear();
            }

            _store.Clear();
        }
    }
}