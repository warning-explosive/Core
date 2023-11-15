namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;

    internal class Queryable<T> : IQueryable<T>,
                                  IOrderedQueryable<T>,
                                  IAsyncEnumerable<T>,
                                  IInsertQueryable<T>,
                                  ICachedInsertQueryable<T>,
                                  IUpdateQueryable<T>,
                                  ISetUpdateQueryable<T>,
                                  IFilteredUpdateQueryable<T>,
                                  ICachedUpdateQueryable<T>,
                                  IDeleteQueryable<T>,
                                  IFilteredDeleteQueryable<T>,
                                  ICachedDeleteQueryable<T>,
                                  ICachedQueryable<T>
    {
        private readonly IAsyncQueryProvider _queryProvider;
        private readonly Expression _expression;

        public Queryable(IAsyncQueryProvider queryProvider, Expression expression)
        {
            _queryProvider = queryProvider;
            _expression = expression;
        }

        public Type ElementType => typeof(T);

        public IQueryProvider Provider => _queryProvider;

        public IAsyncQueryProvider AsyncQueryProvider => _queryProvider;

        public Expression Expression => _expression;

        public IEnumerator<T> GetEnumerator()
        {
            return _queryProvider
                .Execute<IEnumerable<T>>(_expression)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return _queryProvider
                .ExecuteAsync<T>(_expression, cancellationToken)
                .GetAsyncEnumerator(cancellationToken);
        }
    }
}