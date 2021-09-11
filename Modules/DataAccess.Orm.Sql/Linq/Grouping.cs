namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class Grouping<TKey, TValue> : IGrouping<TKey, TValue>
    {
        private readonly IEnumerable<TValue> _values;

        internal Grouping(TKey key, IEnumerable<TValue> values)
        {
            Key = key;

            _values = values;
        }

        public TKey Key { get; }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}