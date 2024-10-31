namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public static class DictionaryExtensions
{
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

    public static TValue GetOrAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TKey, TValue> producer)
    {
        if (dictionary.TryGetValue(key, out var taken))
        {
            return taken;
        }

        var value = producer(key);
        dictionary[key] = value;
        return value;
    }

    public static TValue GetOrAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value)
    {
        if (dictionary.TryGetValue(key, out var taken))
        {
            return taken;
        }

        dictionary[key] = value;
        return value;
    }

    public static void Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        (dictionary as IDictionary<TKey, TValue>).Add(key, value);
    }
}