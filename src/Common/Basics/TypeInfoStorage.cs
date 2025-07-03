namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

internal static class TypeInfoStorage
{
    private static readonly ConcurrentDictionary<string, TypeInfo> Cache = new();

    private static readonly Lazy<IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>>> TypesCache
        = new(InitializeTypesCache, LazyThreadSafetyMode.ExecutionAndPublication);

    internal static bool TryGet(string assemblyName, string typeFullName, [NotNullWhen(true)] out Type? type)
    {
        if (TypesCache.Value.TryGetValue(assemblyName, out var types)
            && types.TryGetValue(typeFullName, out type))
        {
            return true;
        }

        type = null;
        return false;
    }

    internal static TypeInfo Get(Type type) => Cache.GetOrAdd(GetKey(type), static (_, t) => new TypeInfo(t), type);

    private static string GetKey(Type type)
    {
        if (type.IsGenericParameter)
        {
            throw new InvalidOperationException("Type cache doesn't support generic parameters");
        }

        return !type.IsGenericTypeDefinition && type.ContainsGenericParameters
            ? type.ToString()
            : type.FullName ?? throw new InvalidOperationException($"Type cache doesn't support types without {nameof(Type.FullName)}: {type}");
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>> InitializeTypesCache()
    {
        return AssemblyExtensions
            .AllAssembliesFromCurrentDomain()
            .ToDictionary(
                assembly => assembly.GetName().Name!,
                assembly => (IReadOnlyDictionary<string, Type>)assembly
                    .GetTypes()
                    .ToDictionary(type => type.FullName!));
    }
}