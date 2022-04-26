namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Views;

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

        /// <summary>
        /// Gets sql view query
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        /// <returns>Sql view query</returns>
        public static string SqlViewQuery(this Type type, IDependencyContainer dependencyContainer)
        {
            if (!type.IsSqlView())
            {
                throw new InvalidOperationException();
            }

            var viewKeyType = type.UnwrapTypeParameter(typeof(ISqlView<>));

            return dependencyContainer
                .ResolveGeneric(typeof(ISqlViewQueryProvider<,>), type, viewKeyType)
                .CallMethod(nameof(ISqlViewQueryProvider<ISqlView<Guid>, Guid>.GetQuery))
                .Invoke<string>();
        }
    }
}