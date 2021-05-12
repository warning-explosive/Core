namespace SpaceEngineers.Core.DataAccess.Orm.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;

    /// <summary>
    /// Class that represents query to the database
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    internal class Queryable<T> : IQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly IAsyncQueryProvider _queryProvider;
        private readonly Expression _expression;

        /// <summary> .cctor </summary>
        /// <param name="queryProvider">IAsyncQueryProvider</param>
        /// <param name="expression">Expression</param>
        public Queryable(IAsyncQueryProvider queryProvider, Expression expression)
        {
            _queryProvider = queryProvider;
            _expression = expression;
        }

        /// <inheritdoc />
        public Type ElementType => typeof(T);

        /// <inheritdoc />
        public IQueryProvider Provider => _queryProvider;

        /// <inheritdoc />
        public Expression Expression => _expression;

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return _queryProvider
                .Execute<IEnumerable<T>>(_expression)
                .GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken token)
        {
            return _queryProvider
                .ExecuteAsync<T>(_expression, token)
                .GetAsyncEnumerator(token);
        }
    }
}