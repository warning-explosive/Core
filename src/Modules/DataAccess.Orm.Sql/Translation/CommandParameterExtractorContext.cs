namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Basics;

    /// <summary>
    /// CommandParameterExtractorContext
    /// </summary>
    public class CommandParameterExtractorContext
    {
        private readonly Dictionary<string, object> _storage;

        /// <summary> .cctor </summary>
        /// <param name="expression">Expression</param>
        public CommandParameterExtractorContext(Expression expression)
        {
            _storage = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            Expression = expression;
        }

        /// <summary>
        /// Expression
        /// </summary>
        public Expression Expression { get; }

        /// <summary>
        /// GetOrAdd
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="producer">Value producer</param>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Created or existed item</returns>
        public TValue GetOrAdd<TValue>(string key, Func<TValue> producer)
        {
            return (TValue)_storage.GetOrAdd(key, _ => producer() !);
        }
    }
}