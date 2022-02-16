namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Extensions
{
    using System;
    using System.Reflection;
    using Basics;

    /// <summary>
    /// TranslationExtensions
    /// </summary>
    public static class TranslationExtensions
    {
        /// <summary>
        /// Gets properties that represent database table columns
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Columns</returns>
        public static PropertyInfo[] Columns(this Type type)
        {
            return type.GetProperties(
                BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.GetProperty
                | BindingFlags.SetProperty);
        }

        /// <summary>
        /// Gets property that represent database table column
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Column name</param>
        /// <returns>Columns</returns>
        public static PropertyInfo Column(this Type type, string name)
        {
            return type.GetProperty(
                    name,
                    BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.GetProperty
                    | BindingFlags.SetProperty)
                .EnsureNotNull($"Unable to find column {name} in table {type.FullName}");
        }
    }
}