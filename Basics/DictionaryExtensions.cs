namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Dictionary data structure extensions
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Get exact value or add new for specified key
        /// </summary>
        /// <param name="dictionary">Dictionary</param>
        /// <param name="key">Key</param>
        /// <param name="valueFactory">Value factory. If key not presented in collection.</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Value from dictionary</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            var value = valueFactory();
            dictionary[key] = value;
            return value;
        }
    }
}