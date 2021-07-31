namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    /// <summary>
    /// Grouped query
    /// </summary>
    public class GroupedQuery : IQuery
    {
        /// <summary> .cctor </summary>
        /// <param name="keysQuery">Keys query</param>
        /// <param name="valuesQuery">Values query</param>
        public GroupedQuery(string keysQuery, string valuesQuery)
        {
            KeysQuery = keysQuery;
            ValuesQuery = valuesQuery;
        }

        /// <summary>
        /// Keys query
        /// </summary>
        public string KeysQuery { get; }

        /// <summary>
        /// Keys query parameters object
        /// </summary>
        public object? KeysParameters { get; set; }

        /// <summary>
        /// Values query
        /// </summary>
        public string ValuesQuery { get; }

        /// <summary>
        /// Values query parameters object
        /// </summary>
        public object? ValuesParameters { get; set; }
    }
}