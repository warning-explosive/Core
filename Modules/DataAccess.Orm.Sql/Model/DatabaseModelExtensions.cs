namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Api.Sql;
    using Basics;

    /// <summary>
    /// DatabaseModelExtensions
    /// </summary>
    public static class DatabaseModelExtensions
    {
        /// <summary>
        /// Is type sql view
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type is sql view or not</returns>
        public static bool IsSqlView(this Type type)
        {
            return type.IsSubclassOfOpenGeneric(typeof(ISqlView<>))
                && type.IsConcreteType();
        }
    }
}