namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    /// <summary>
    /// Flat query
    /// </summary>
    public sealed class FlatQuery : IQuery
    {
        /// <summary> .cctor </summary>
        /// <param name="query">Query</param>
        public FlatQuery(string query)
        {
            Query = query;
        }

        /// <summary>
        /// Query
        /// </summary>
        public string Query { get; }

        /// <summary>
        /// Query parameters object
        /// </summary>
        public object? Parameters { get; set; }
    }
}