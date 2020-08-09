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
        public static ITypeInfo Get(Type type) => Cache.GetOrAdd(type.FullName, _ => new TypeInfo(type));
    }
}