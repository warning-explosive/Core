namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Extensions
{
    using System;
    using System.Reflection;

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
    }
}