namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using Model;

    /// <summary>
    /// TranslationExtensions
    /// </summary>
    public static class TranslationExtensions
    {
        private static readonly ConcurrentDictionary<Type, ColumnProperty[]> ColumnsCache
            = new ConcurrentDictionary<Type, ColumnProperty[]>();

        /// <summary>
        /// Gets properties that represent database table columns
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Columns</returns>
        public static ColumnProperty[] Columns(this Type type)
        {
            return ColumnsCache.GetOrAdd(type, key => GetColumns(key).ToArray());

            static IEnumerable<ColumnProperty> GetColumns(Type type)
            {
                return DeclaredColumns(type)
                    .Join(ReflectedColumns(type),
                        declared => declared.Name,
                        reflected => reflected.Name,
                        (declared, reflected) => new ColumnProperty(declared, reflected),
                        StringComparer.OrdinalIgnoreCase);
            }

            static IEnumerable<PropertyInfo> ReflectedColumns(Type type)
            {
                return type
                    .GetProperties(
                        BindingFlags.Public
                        | BindingFlags.Instance
                        | BindingFlags.GetProperty
                        | BindingFlags.SetProperty);
            }

            static IEnumerable<PropertyInfo> DeclaredColumns(Type type)
            {
                return type
                    .GetProperties(
                        BindingFlags.Public
                        | BindingFlags.Instance
                        | BindingFlags.GetProperty
                        | BindingFlags.SetProperty
                        | BindingFlags.DeclaredOnly)
                    .Concat(BaseDeclaredColumns(type))
                    .Where(property => property.CanRead && (property.CanWrite || type.IsAnonymous()));
            }

            static IEnumerable<PropertyInfo> BaseDeclaredColumns(Type type)
            {
                return type.BaseType == null
                    ? Enumerable.Empty<PropertyInfo>()
                    : DeclaredColumns(type.BaseType);
            }
        }

        /// <summary>
        /// Gets property that represent database table column
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Column name</param>
        /// <returns>Columns</returns>
        public static ColumnProperty Column(this Type type, string name)
        {
            return Columns(type).SingleOrDefault(property => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                   ?? throw new InvalidOperationException($"Unable to find column {name} in table {type.FullName}");
        }
    }
}