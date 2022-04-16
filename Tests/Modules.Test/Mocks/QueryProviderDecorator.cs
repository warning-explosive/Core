namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System.Linq;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;

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
            return Decoratee.CreateQuery(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return Decoratee.CreateQuery<TElement>(expression);
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