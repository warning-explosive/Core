namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Linq;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
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
            return type.IsSubclassOfOpenGeneric(typeof(ISqlView<>));
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

        /// <summary>
        /// Gets schema name
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Schema name</returns>
        public static string SchemaName(this Type type)
        {
            return type
                .Assembly
                .GetName()
                .Name
                .Replace(".", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets schema name for mtm-table
        /// </summary>
        /// <param name="left">Left relation</param>
        /// <param name="right">Right relation</param>
        /// <returns>Schema name</returns>
        public static string MtmSchemaName(Type left, Type right)
        {
            return new[] { left.SchemaName(), right.SchemaName() }
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToString("_");
        }
    }
}