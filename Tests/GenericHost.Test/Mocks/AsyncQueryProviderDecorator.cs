namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using DataAccess.Orm.Linq;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;

    [ManuallyRegisteredComponent(nameof(DataAccess))]
    internal class AsyncQueryProviderDecorator : IAsyncQueryProvider,
                                                 IDecorator<IAsyncQueryProvider>
    {
        private readonly QueryExpressionsCollector _collector;

        public AsyncQueryProviderDecorator(IAsyncQueryProvider decoratee, QueryExpressionsCollector collector)
        {
            _collector = collector;
            Decoratee = decoratee;
        }

        public IAsyncQueryProvider Decoratee { get; }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            var itemType = expression.Type.UnwrapTypeParameter(typeof(IQueryable<>));

            return (IQueryable)Activator.CreateInstance(
                typeof(Queryable<>).MakeGenericType(itemType),
                this,
                expression) !;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return (IQueryable<TElement>)((IQueryProvider)this).CreateQuery(expression);
        }

        public object? Execute(Expression expression)
        {
            _collector.Collect(expression);
            return Decoratee.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            _collector.Collect(expression);
            return Decoratee.Execute<TResult>(expression);
        }

        public Task<T> ExecuteScalarAsync<T>(Expression expression, CancellationToken token)
        {
            _collector.Collect(expression);
            return Decoratee.ExecuteScalarAsync<T>(expression, token);
        }

        public IAsyncEnumerable<T> ExecuteAsync<T>(Expression expression, CancellationToken token)
        {
            _collector.Collect(expression);
            return Decoratee.ExecuteAsync<T>(expression, token);
        }
    }
}