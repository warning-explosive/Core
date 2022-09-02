namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Attributes;

    /// <summary>
    /// TypeInfo storage
    /// </summary>
    public sealed class TypeInfoStorage
    {
        private static readonly ConcurrentDictionary<string, ITypeInfo> Cache = new ConcurrentDictionary<string, ITypeInfo>();

        private static readonly Lazy<IReadOnlyDictionary<Type, IReadOnlyCollection<Type>>> BeforeAttributeMap
            = new Lazy<IReadOnlyDictionary<Type, IReadOnlyCollection<Type>>>(InitializeBeforeAttributeMap, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets or builds type info from storage
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>ITypeInfo</returns>
        public static ITypeInfo Get(Type type) => Cache.GetOrAdd(GetKey(type), _ => new TypeInfo(type));

        internal static IEnumerable<Type> ExtractDependencies(Type type)
        {
            var byAfterAttribute = type
                 .GetCustomAttribute<AfterAttribute>()
                ?.Types ?? Enumerable.Empty<Type>();

            var byBeforeAttribute = BeforeAttributeMap.Value.TryGetValue(type, out var value)
                ? value
                : Enumerable.Empty<Type>();

            return byAfterAttribute.Concat(byBeforeAttribute);
        }

        private static string GetKey(Type type)
        {
            if (type.IsGenericParameter)
            {
                throw new InvalidOperationException("Type cache doesn't support generic parameters");
            }

            return !type.IsGenericTypeDefinition && type.ContainsGenericParameters
                ? type.ToString()
                : type.FullName.EnsureNotNull($"Type cache doesn't support types without {nameof(Type.FullName)}: {type}");
        }

        private static IReadOnlyDictionary<Type, IReadOnlyCollection<Type>> InitializeBeforeAttributeMap()
        {
            return AssembliesExtensions
                .AllAssembliesFromCurrentDomain()
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(GetTypes)
                .Select(type =>
                {
                    var attribute = type.GetCustomAttribute<BeforeAttribute>();
                    return (type, attribute);
                })
                .Where(pair => pair.attribute != null)
                .SelectMany(pair => pair.attribute.Types.Select(before => (before, pair.type)))
                .GroupBy(pair => pair.before, pair => pair.type)
                .ToDictionary(grp => grp.Key, grp => grp.ToList() as IReadOnlyCollection<Type>);

            static IEnumerable<Type> GetTypes(Assembly assembly)
            {
                return ExecutionExtensions
                    .Try<IEnumerable<Type>>(assembly.GetTypes)
                    .Catch<ReflectionTypeLoadException>()
                    .Catch<FileNotFoundException>()
                    .Invoke(_ => Enumerable.Empty<Type>());
            }
        }
    }
}