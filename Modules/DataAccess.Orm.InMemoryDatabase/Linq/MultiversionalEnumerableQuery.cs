namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class MultiversionalEnumerableQuery<T> : IEnumerable<T>
    {
        private readonly Func<DateTime, IEnumerable<T>> _producer;

        public MultiversionalEnumerableQuery(Func<DateTime, IEnumerable<T>> producer)
        {
            _producer = producer;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var enumerable = _producer.Invoke(DateTime.UtcNow);
            return enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}