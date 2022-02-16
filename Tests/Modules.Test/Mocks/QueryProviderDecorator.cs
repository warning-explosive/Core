namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using DataAccess.Orm.Linq;

    [ManuallyRegisteredComponent(nameof(DataAccess))]
    internal class QueryProviderDecorator : IQueryProvider,
                                            IDecorator<IQueryProvider>
    {
        private readonly QueryExpressionsCollector _collector;

        public QueryProviderDecorator(IQueryProvider decoratee, QueryExpressionsCollector collector)
        {
            _collector = collector;
            Decoratee = decoratee;
        }

        public IQueryProvider Decoratee { get; }

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
    }
}