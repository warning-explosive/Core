namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// System.Collections.Generic.Dictionary extensions
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Add or update
        /// </summary>
        /// <param name="dictionary">Dictionary</param>
        /// <param name="key">Key</param>
        /// <param name="add">Add producer</param>
        /// <param name="update">Update producer</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Added or updated value</returns>
        public static TValue AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> add,
            Func<TKey, TValue, TValue> update)
        {
            if (dictionary.ContainsKey(key))
            {
                var value = dictionary[key];
                var newValue = update(key, value);
                dictionary[key] = newValue;
                return newValue;
            }
            else
            {
                var newValue = add(key);
                dictionary[key] = newValue;
                return newValue;
            }
        }

        /// <summary>
        /// Get or add
        /// </summary>
        /// <param name="dictionary">Dictionary</param>
        /// <param name="key">Key</param>
        /// <param name="producer">Value producer</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Existed or produced value</returns>
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> producer)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            value = producer(key);
            dictionary[key] = value;
            return value;
        }

        /// <summary>
        /// Add entry into ConcurrentDictionary
        /// </summary>
        /// <param name="dictionary">ConcurrentDictionary</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        public static void Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            (dictionary as IDictionary<TKey, TValue>).Add(key, value);
        }
    }
}