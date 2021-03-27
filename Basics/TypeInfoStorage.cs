namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// TypeInfo storage
    /// </summary>
    public sealed class TypeInfoStorage
    {
        private static readonly ConcurrentDictionary<string, ITypeInfo> Cache = new ConcurrentDictionary<string, ITypeInfo>();

        /// <summary>
        /// Get ITypeInfo
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>ITypeInfo</returns>
        public static ITypeInfo Get(Type type) => Cache.GetOrAdd(GetKey(type), _ => new TypeInfo(type));

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
    }
}