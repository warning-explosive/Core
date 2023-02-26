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
        /// Does type represent sql view
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Check result</returns>
        public static bool IsSqlView(this Type type)
        {
            return type.IsSubclassOfOpenGeneric(typeof(ISqlView<>))
                && type.IsConcreteType();
        }

        /// <summary>
        /// Does type represent mtm table
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Check result</returns>
        public static bool IsMtmTable(this Type type)
        {
            return type.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>));
        }
    }
}