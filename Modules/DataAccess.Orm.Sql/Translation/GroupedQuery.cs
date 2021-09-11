namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using Orm.Linq;

    /// <summary>
    /// Grouped query
    /// </summary>
    public class GroupedQuery : IQuery
    {
        /// <summary> .cctor </summary>
        /// <param name="keysQuery">Keys query</param>
        /// <param name="keysQueryParameters">Keys query parameters object</param>
        /// <param name="valuesExpressionProducer">Values expression producer</param>
        public GroupedQuery(string keysQuery,
            object? keysQueryParameters,
            Func<IReadOnlyDictionary<string, object?>, IIntermediateExpression> valuesExpressionProducer)
        {
            KeysQuery = keysQuery;
            KeysQueryParameters = keysQueryParameters;
            ValuesExpressionProducer = valuesExpressionProducer;
        }

        /// <summary>
        /// Keys query
        /// </summary>
        public string KeysQuery { get; }

        /// <summary>
        /// Keys query parameters object
        /// </summary>
        public object? KeysQueryParameters { get; }

        /// <summary>
        /// Values expression producer
        /// </summary>
        public Func<IReadOnlyDictionary<string, object?>, IIntermediateExpression> ValuesExpressionProducer { get; }
    }
}