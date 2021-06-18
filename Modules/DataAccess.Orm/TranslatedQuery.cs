namespace SpaceEngineers.Core.DataAccess.Orm
{
    /// <summary>
    /// Translated query
    /// </summary>
    public sealed class TranslatedQuery
    {
        /// <summary> .cctor </summary>
        /// <param name="query">Translated query</param>
        public TranslatedQuery(string query)
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