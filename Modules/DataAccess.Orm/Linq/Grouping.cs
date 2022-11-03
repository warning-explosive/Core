namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Grouping
    /// </summary>
    /// <typeparam name="TKey">TKey typ-argument</typeparam>
    /// <typeparam name="TValue">TValue type-argument</typeparam>
    public class Grouping<TKey, TValue> : IGrouping<TKey, TValue>
    {
        private readonly IEnumerable<TValue> _values;

        /// <summary> .cctor </summary>
        /// <param name="key">Key</param>
        /// <param name="values">Values</param>
        public Grouping(TKey key, IEnumerable<TValue> values)
        {
            Key = key;

            _values = values;
        }

        /// <inheritdoc />
        public TKey Key { get; }

        /// <inheritdoc />
        public IEnumerator<TValue> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}