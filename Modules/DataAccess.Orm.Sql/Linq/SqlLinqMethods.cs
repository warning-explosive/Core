namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;
    using System.Reflection;
    using Basics;
    using Execution;

    /// <summary>
    /// SqlLinqMethods
    /// </summary>
    public static class SqlLinqMethods
    {
        private const string CouldNotFindMethodFormat = "Could not find {0} method";

        private static MethodInfo? _like;
        private static MethodInfo? _isNull;
        private static MethodInfo? _isNotNull;

        /// <summary>
        /// SqlExpressionsExtensions.Like
        /// </summary>
        /// <returns>SqlExpressionsExtensions.Like MethodInfo</returns>
        public static MethodInfo Like()
        {
            return _like ??= new MethodFinder(typeof(SqlExpressionsExtensions),
                    nameof(SqlExpressionsExtensions.Like),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    ArgumentTypes = new[] { typeof(string), typeof(string) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SqlExpressionsExtensions.Like()"));
        }

        /// <summary>
        /// SqlExpressionsExtensions.IsNull
        /// </summary>
        /// <returns>SqlExpressionsExtensions.IsNull MethodInfo</returns>
        public static MethodInfo IsNull()
        {
            return _isNull ??= new MethodFinder(typeof(SqlExpressionsExtensions),
                    nameof(SqlExpressionsExtensions.IsNull),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    ArgumentTypes = new[] { typeof(object) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SqlExpressionsExtensions.IsNull()"));
        }

        /// <summary>
        /// SqlExpressionsExtensions.IsNotNull
        /// </summary>
        /// <returns>SqlExpressionsExtensions.IsNotNull MethodInfo</returns>
        public static MethodInfo IsNotNull()
        {
            return _isNotNull ??= new MethodFinder(typeof(SqlExpressionsExtensions),
                    nameof(SqlExpressionsExtensions.IsNotNull),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    ArgumentTypes = new[] { typeof(object) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SqlExpressionsExtensions.IsNotNull()"));
        }
    }
}