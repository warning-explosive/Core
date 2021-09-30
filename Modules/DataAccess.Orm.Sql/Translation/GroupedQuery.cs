namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using Expressions;
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
            IReadOnlyDictionary<string, (Type, object?)> keysQueryParameters,
            Func<IReadOnlyDictionary<string, (Type, object?)>, IIntermediateExpression> valuesExpressionProducer)
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
        public IReadOnlyDictionary<string, (Type, object?)> KeysQueryParameters { get; }

        /// <summary>
        /// Values expression producer
        /// </summary>
        public Func<IReadOnlyDictionary<string, (Type, object?)>, IIntermediateExpression> ValuesExpressionProducer { get; }
    }
}