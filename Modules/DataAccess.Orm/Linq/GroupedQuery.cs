namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    /// <summary>
    /// Grouped query
    /// </summary>
    public class GroupedQuery : IQuery
    {
        /// <summary> .cctor </summary>
        /// <param name="keyQuery">Key query</param>
        /// <param name="keyQueryParameters">Key query parameters object</param>
        /// <param name="valueQuery">Value query</param>
        /// <param name="valueQueryParameters">Value query parameters object</param>
        public GroupedQuery(
            string keyQuery,
            object? keyQueryParameters,
            string valueQuery,
            object? valueQueryParameters)
        {
            KeyQuery = keyQuery;
            KeyQueryParameters = keyQueryParameters;
            ValueQuery = valueQuery;
            ValueQueryParameters = valueQueryParameters;
        }

        /// <summary>
        /// Key query
        /// </summary>
        public string KeyQuery { get; }

        /// <summary>
        /// Key query parameters object
        /// </summary>
        public object? KeyQueryParameters { get; }

        /// <summary>
        /// Value query
        /// </summary>
        public string ValueQuery { get; }

        /// <summary>
        /// Value query parameters object
        /// </summary>
        public object? ValueQueryParameters { get; }
    }
}