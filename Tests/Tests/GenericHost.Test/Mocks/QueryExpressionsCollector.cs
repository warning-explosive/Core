namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using DataAccess.Orm.Sql.Linq;

    [ManuallyRegisteredComponent(nameof(LinqToSqlTests))]
    internal class QueryExpressionsCollector : IResolvable<QueryExpressionsCollector>,
                                               IDisposable
    {
        private readonly IAsyncQueryProvider _queryProvider;
        private readonly EventHandler<ExecutedExpressionEventArgs> _subscription;

        public QueryExpressionsCollector(IAsyncQueryProvider queryProvider)
        {
            _queryProvider = queryProvider;
            _subscription = Collect;

            _queryProvider.ExpressionExecuted += _subscription;

            Expressions = new ConcurrentQueue<Expression>();
        }

        public ConcurrentQueue<Expression> Expressions { get; }

        public void Dispose()
        {
            _queryProvider.ExpressionExecuted -= _subscription;
        }

        private void Collect(object? sender, ExecutedExpressionEventArgs args)
        {
            Expressions.Enqueue(args.Expression);
        }
    }
}