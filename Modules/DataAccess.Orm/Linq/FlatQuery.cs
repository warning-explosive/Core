namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using Abstractions;

    /// <summary>
    /// Flat query
    /// </summary>
    public sealed class FlatQuery : IQuery
    {
        /// <summary> .cctor </summary>
        /// <param name="query">Query</param>
        /// <param name="queryParameters">Query parameters object</param>
        public FlatQuery(string query, object? queryParameters)
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
        public object? QueryParameters { get; }
    }
}