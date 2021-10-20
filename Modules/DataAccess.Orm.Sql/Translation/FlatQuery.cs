namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using Orm.Linq;

    /// <summary>
    /// Flat query
    /// </summary>
    public sealed class FlatQuery : IQuery
    {
        /// <summary> .cctor </summary>
        /// <param name="query">Query</param>
        /// <param name="queryParameters">Query parameters object</param>
        public FlatQuery(string query, IReadOnlyDictionary<string, object?> queryParameters)
        {
            Query = query;
            QueryParameters = queryParameters;
        }

        /// <summary>
        /// Query
        /// </summary>
        public string Query { get; }

        /// <summary>
        /// Query parameters object
        /// </summary>
        public IReadOnlyDictionary<string, object?> QueryParameters { get; }
    }
}