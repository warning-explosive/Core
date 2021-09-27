namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Database
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Exceptions;
    using Api.Model;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Connection;
    using CrossCuttingConcerns.Api.Abstractions;
    using Exceptions;
    using Linq;
    using Settings;

    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    [Component(EnLifestyle.Singleton)]
    internal class InMemoryDatabase : IInMemoryDatabase
    {
        private static readonly HashSet<IsolationLevel> SupportedIsolationLevels = new HashSet<IsolationLevel>
        {
            IsolationLevel.ReadCommitted,
            IsolationLevel.ReadUncommitted,
            IsolationLevel.Snapshot
        };

        private readonly ISettingsProvider<InMemoryDatabaseSettings> _settingsProvider;

        private readonly ConcurrentDictionary<Guid, IAdvancedDbTransaction> _transactions;

        private readonly object _sync;

        private DateTime _timestamp;

        private ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>> _store;

        public InMemoryDatabase(ISettingsProvider<InMemoryDatabaseSettings> settingsProvider)
        {
            _settingsProvider = settingsProvider;

            _transactions = new ConcurrentDictionary<Guid, IAdvancedDbTransaction>();
            _sync = new object();

            _timestamp = DateTime.UtcNow;
            _store = new ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>>();
        }

        #region IIdentifiedDatabase

        public string Name => _settingsProvider.Get(CancellationToken.None).Result.Database;

        #endregion

        #region IInitializableDatabase

        public Task CreateTable(Type table, CancellationToken token)
        {
            _store.GetOrAdd(table, _ => new ConcurrentDictionary<object, ConcurrentStack<Entry>>());
            return Task.CompletedTask;
        }

        #endregion

        #region ITransactionalDatabase

        public IAdvancedDbTransaction BeginTransaction(InMemoryDbConnection connection, IsolationLevel isolationLevel)
        {
            if (!SupportedIsolationLevels.Contains(isolationLevel))
            {
                throw new NotSupportedException($"Not supported isolation level: {isolationLevel}");
            }

            var transaction = new InMemoryDbTransaction(this, connection, isolationLevel);

            _transactions.Add(transaction.Id, transaction);

            return transaction;
        }

        public void EndTransaction(IAdvancedDbTransaction transaction)
        {
            try
            {
                ApplyChanges(transaction);
            }
            finally
            {
                _transactions.Remove(transaction.Id, out _);
            }
        }

        #endregion

        #region IReadableDatabase

        public Entry? SingleOrDefault<TEntity, TKey>(TKey primaryKey, IAdvancedDbTransaction transaction)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            var now = DateTime.UtcNow;

            return transaction.IsolationLevel switch
            {
                IsolationLevel.ReadCommitted => TakeLastVersion(ReadFromTransactions(new[] { transaction }, primaryKey, now), ReadFromStore(_store, primaryKey, now)),
                IsolationLevel.ReadUncommitted => TakeLastVersion(ReadFromTransactions(_transactions.Values, primaryKey, now), ReadFromStore(_store, primaryKey, now)),
                IsolationLevel.Snapshot => TakeLastVersion(ReadFromTransactions(new[] { transaction }, primaryKey, now), ReadFromStore(_store, primaryKey, transaction.Timestamp)),
                _ => throw new NotSupportedException($"Not supported isolation level: {transaction.IsolationLevel}")
            };

            static Entry? TakeLastVersion(params IEnumerable<Entry>[] changeSources)
            {
                return changeSources
                    .Aggregate((prev, next) => prev.Concat(next))
                    .OrderByDescending(entry => entry.Timestamp)
                    .FirstOrDefault();
            }

            static IEnumerable<Entry> ReadFromTransactions(
                IEnumerable<ITrackableDbTransaction> transactions,
                TKey primaryKey,
                DateTime timestamp)
            {
                var versionsFromTransactions = transactions
                    .Select(transaction =>
                    {
                        _ = transaction
                            .Changes
                            .TryGetValue(typeof(TEntity), out var collection);

                        return collection;
                    })
                    .Where(collection => collection != null)
                    .Select(collection =>
                    {
                        _ = collection
                            .TryGetValue(primaryKey, out var versions);

                        return versions;
                    })
                    .Where(versions => versions != null)
                    .SelectMany(versions => MostRelevantEntry(versions, timestamp));

                return MostRelevantEntry(versionsFromTransactions, timestamp);
            }

            static IEnumerable<Entry> ReadFromStore(
                ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>> store,
                TKey primaryKey,
                DateTime timestamp)
            {
                if (!store.TryGetValue(typeof(TEntity), out var collection))
                {
                    return Enumerable.Empty<Entry>();
                }

                if (!collection.TryGetValue(primaryKey, out var versions))
                {
                    return Enumerable.Empty<Entry>();
                }

                return MostRelevantEntry(versions, timestamp);
            }

            static IEnumerable<Entry> MostRelevantEntry(
                IEnumerable<Entry> versions,
                DateTime timestamp)
            {
                return versions
                    .OrderByDescending(entry => entry.Timestamp)
                    .Where(entry => entry.Timestamp <= timestamp)
                    .Take(1)
                    .Where(entry => entry.EntryType != EnEntryType.Deleted);
            }
        }

        public IQueryable<TEntity> All<TEntity, TKey>(InMemoryDbTransaction transaction)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return new EnumerableQuery<TEntity>(new MultiversionalEnumerableQuery<TEntity>(ReadAllEnumerable));

            IEnumerable<TEntity> ReadAllEnumerable(DateTime now)
            {
                return transaction.IsolationLevel switch
                {
                    IsolationLevel.ReadCommitted => TakeLastVersion(now, ReadFromTransactions(now, new[] { transaction }), ReadFromStore(now, _store)),
                    IsolationLevel.ReadUncommitted => TakeLastVersion(now, ReadFromTransactions(now, _transactions.Values), ReadFromStore(now, _store)),
                    IsolationLevel.Snapshot => TakeLastVersion(now, ReadFromTransactions(now, new[] { transaction }), ReadFromStore(transaction.Timestamp, _store)),
                    _ => throw new NotSupportedException($"Not supported isolation level: {transaction.IsolationLevel}")
                };
            }

            static IEnumerable<TEntity> TakeLastVersion(DateTime timestamp, params IEnumerable<Entry>[] changeSources)
            {
                return changeSources
                    .Aggregate((prev, next) => prev.Concat(next))
                    .GroupBy(entry => (entry.Type, entry.PrimaryKey))
                    .SelectMany(grp => MostRelevantEntry(grp, timestamp))
                    .Select(entry => (TEntity)entry.Entity);
            }

            static IEnumerable<Entry> ReadFromTransactions(
                DateTime timestamp,
                IEnumerable<ITrackableDbTransaction> transactions)
            {
                return transactions
                    .Select(transaction =>
                    {
                        _ = transaction
                            .Changes
                            .TryGetValue(typeof(TEntity), out var collection);

                        return collection;
                    })
                    .Where(collection => collection != null)
                    .SelectMany(changes => changes.Values)
                    .SelectMany(versions => MostRelevantEntry(versions, timestamp))
                    .GroupBy(entry => (entry.Type, entry.PrimaryKey))
                    .SelectMany(grp => MostRelevantEntry(grp, timestamp));
            }

            static IEnumerable<Entry> ReadFromStore(
                DateTime timestamp,
                ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>> store)
            {
                if (store.TryGetValue(typeof(TEntity), out var collection))
                {
                    foreach (var (_, versions) in collection)
                    {
                        foreach (var entry in MostRelevantEntry(versions, timestamp))
                        {
                            yield return entry;
                        }
                    }
                }
            }

            static IEnumerable<Entry> MostRelevantEntry(
                IEnumerable<Entry> versions,
                DateTime timestamp)
            {
                return versions
                    .OrderByDescending(entry => entry.Timestamp)
                    .Where(entry => entry.Timestamp <= timestamp)
                    .Take(1)
                    .Where(entry => entry.EntryType != EnEntryType.Deleted);
            }
        }

        #endregion

        #region IWritableDatabase

        public Entry Create<TEntity, TKey>(TEntity entity, IAdvancedDbTransaction transaction)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            var entry = SingleOrDefault<TEntity, TKey>(entity.PrimaryKey, transaction);

            if (entry != default)
            {
                throw new ConstraintViolationException(typeof(TEntity), entity.PrimaryKey);
            }

            return new Entry(entity.PrimaryKey, entity, typeof(TEntity), transaction.Timestamp, EnEntryType.Created);
        }

        public Entry Update<TEntity, TKey>(TEntity entity, IAdvancedDbTransaction transaction)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            var entry = SingleOrDefault<TEntity, TKey>(entity.PrimaryKey, transaction);

            if (entry == default)
            {
                throw new EntityNotFoundException<TEntity, TKey>(entity.PrimaryKey);
            }

            return new Entry(entity.PrimaryKey, entity, typeof(TEntity), transaction.Timestamp, EnEntryType.Updated);
        }

        public Entry Delete<TEntity, TKey>(TKey primaryKey, IAdvancedDbTransaction transaction)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            var entry = SingleOrDefault<TEntity, TKey>(primaryKey, transaction);

            if (entry == default)
            {
                throw new EntityNotFoundException<TEntity, TKey>(primaryKey);
            }

            return new Entry(primaryKey, entry.Entity, typeof(TEntity), transaction.Timestamp, EnEntryType.Deleted);
        }

        #endregion

        #region Internals

        private void ApplyChanges(ITrackableDbTransaction transaction)
        {
            DateTime timestamp;
            ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>> snapshot;

            lock (_sync)
            {
                timestamp = _timestamp;
                snapshot = TakeSnapshot(_store);
            }

            Apply(snapshot, transaction.Changes);

            lock (_sync)
            {
                if (timestamp == _timestamp)
                {
                    _store = snapshot;
                    _timestamp = timestamp;
                }
                else
                {
                    throw new ConcurrencyControlException("Storage", _timestamp, timestamp);
                }
            }

            static ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>> TakeSnapshot(
                ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>> store)
            {
                var snapshot = new ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>>();

                var flatten = store
                    .SelectMany(collections => collections.Value
                        .SelectMany(versions => versions.Value.Reverse()));

                foreach (var collectionType in store.Keys)
                {
                    _ = snapshot.GetOrAdd(
                        collectionType,
                        static _ => new ConcurrentDictionary<object, ConcurrentStack<Entry>>());
                }

                foreach (var entry in flatten)
                {
                    var collection = snapshot.GetOrAdd(
                        entry.Type,
                        static _ => new ConcurrentDictionary<object, ConcurrentStack<Entry>>());

                    var versions = collection.GetOrAdd(
                        entry.PrimaryKey,
                        static _ => new ConcurrentStack<Entry>());

                    versions.Push(entry);
                }

                return snapshot;
            }

            static void Apply(
                ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>> store,
                ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>> changes)
            {
                foreach (var (entityType, changesCollection) in changes)
                {
                    var collection = store[entityType];

                    foreach (var changesStack in changesCollection.Values)
                    {
                        foreach (var nextEntry in changesStack.Reverse())
                        {
                            var versions = collection.GetOrAdd(
                                nextEntry.PrimaryKey,
                                _ => new ConcurrentStack<Entry>());

                            if (versions.TryPeek(out var previousEntry)
                                && previousEntry.Timestamp > nextEntry.Timestamp)
                            {
                                throw new ConcurrencyControlException($"Entity {entityType.Name}", previousEntry.Timestamp, nextEntry.Timestamp);
                            }

                            versions.Push(nextEntry);
                        }
                    }
                }
            }
        }

        #endregion
    }
}